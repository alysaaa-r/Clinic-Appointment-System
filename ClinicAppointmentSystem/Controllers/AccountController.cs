// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.ViewModels;

namespace ClinicAppointmentSystem.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<AccountController> _logger;

    public AccountController(AppDbContext db, ILogger<AccountController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        bool emailTaken = await _db.Admins.AnyAsync(a => a.Email == vm.Email)
                       || await _db.Users.AnyAsync(u => u.Email == vm.Email);
        if (emailTaken)
        {
            ModelState.AddModelError("", "Email is already registered.");
            return View(vm);
        }

        var (hash, salt) = PasswordHasher.HashPassword(vm.Password);

        if (vm.Role == "Admin")
        {
            var admin = new Admin
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsApproved = false,
                IsMainAdmin = false,
                CreatedBy = "self"
            };
            _db.Admins.Add(admin);
        }
        else
        {
            var user = new User
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsApproved = false,
                CreatedBy = "self"
            };
            _db.Users.Add(user);
        }

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Registration submitted. Please wait for main admin approval.";
        return RedirectToAction("Login");
    }


    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Try Admin first
        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == vm.Email);
        if (admin != null)
        {
            if (!PasswordHasher.Verify(vm.Password, admin.PasswordHash, admin.PasswordSalt))
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(vm);
            }

            if (!admin.IsApproved)
            {
                ModelState.AddModelError("", "Your account is pending approval.");
                return View(vm);
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("IsApproved", admin.IsApproved ? "true" : "false"),
            new Claim("IsMainAdmin", admin.IsMainAdmin ? "true" : "false")
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToAction("Index", "AdminDashboard");
        }

        // Try User
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
        if (user == null || !PasswordHasher.Verify(vm.Password, user.PasswordHash, user.PasswordSalt))
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View(vm);
        }

        if (!user.IsApproved)
        {
            ModelState.AddModelError("", "Your account is pending approval.");
            return View(vm);
        }

        var claimsU = new List<Claim>
    {
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, "User"),
        new Claim("IsApproved", user.IsApproved ? "true" : "false")
    };

        var identityU = new ClaimsIdentity(claimsU, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identityU));
        return RedirectToAction("Index", "UserDashboard");
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    // Forgot Password
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var exists = await _db.Users.AnyAsync(u => u.Email == vm.Email) || await _db.Admins.AnyAsync(a => a.Email == vm.Email);
        if (!exists)
        {
            // Do not reveal existence
            TempData["Msg"] = "If the email exists, a reset link has been created.";
            return RedirectToAction("Login");
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "");
        var prt = new PasswordResetToken
        {
            Email = vm.Email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedBy = "system"
        };
        _db.PasswordResetTokens.Add(prt);
        await _db.SaveChangesAsync();

        // For demo: show reset link on screen (replace with SMTP send)
        TempData["ResetLink"] = Url.Action("ResetPassword", "Account", new { token, email = vm.Email }, Request.Scheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token, string email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email)) return BadRequest();
        var exists = await _db.PasswordResetTokens.AnyAsync(t => t.Token == token && t.Email == email && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        if (!exists) return BadRequest("Invalid or expired token.");
        var vm = new ResetPasswordViewModel { Token = token, Email = email };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var token = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == vm.Token && t.Email == vm.Email && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        if (token is null) return BadRequest("Invalid or expired token.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
        if (user is null)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == vm.Email);
            if (admin is null) return BadRequest("Account not found.");
            admin.PasswordHash = PasswordHasher.Hash(vm.NewPassword);
        }
        else
        {
            user.PasswordHash = PasswordHasher.Hash(vm.NewPassword);
        }

        token.IsUsed = true;
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Password reset successful. Please login.";
        return RedirectToAction("Login");
    }
}