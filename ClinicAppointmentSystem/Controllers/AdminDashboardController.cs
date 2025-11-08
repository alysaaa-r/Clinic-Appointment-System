// Controllers/AdminDashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : Controller
{
    private readonly AppDbContext _db;
    public AdminDashboardController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        var pendingUsers = await _db.Users.Where(u => !u.IsApproved).ToListAsync();
        var pendingAdmins = await _db.Admins.Where(a => !a.IsApproved && !a.IsMainAdmin).ToListAsync();
        var upcomingAppointments = await _db.Appointments
            .Include(a => a.Patient)
            .Where(a => a.ScheduledAt >= DateTime.Today)
            .OrderBy(a => a.ScheduledAt)
            .Take(10).ToListAsync();

        ViewBag.PendingUsers = pendingUsers;
        ViewBag.PendingAdmins = pendingAdmins;
        ViewBag.UpcomingAppointments = upcomingAppointments;

        return View();
    }

    // Approvals - Main Admin only
    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> ApproveUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.IsApproved = true;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> RejectUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> ApproveAdmin(int id)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin is null || admin.IsMainAdmin) return NotFound();
        admin.IsApproved = true;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> RejectAdmin(int id)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin is null || admin.IsMainAdmin) return NotFound();
        _db.Admins.Remove(admin);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // List/edit/delete Users & Admins (simple)
    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(users);
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> Admins()
    {
        var admins = await _db.Admins.Where(a => !a.IsMainAdmin).OrderByDescending(a => a.CreatedAt).ToListAsync();
        return View(admins);
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> EditUser(int id) => View(await _db.Users.FindAsync(id));

    [HttpPost, Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> EditUser(User model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Users.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> EditAdmin(int id) => View(await _db.Admins.FindAsync(id));

    [HttpPost, Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> EditAdmin(Admin model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.IsMainAdmin) return BadRequest();
        _db.Admins.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Admins");
    }

    [Authorize(Policy = "MainAdminOnly")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin is null || admin.IsMainAdmin) return NotFound();
        _db.Admins.Remove(admin);
        await _db.SaveChangesAsync();
        return RedirectToAction("Admins");
    }

    // Appointments management
    [Authorize] // any approved admin
    public async Task<IActionResult> Appointments()
    {
        var list = await _db.Appointments.Include(a => a.Patient).OrderByDescending(a => a.ScheduledAt).ToListAsync();
        return View(list);
    }

    [Authorize]
    public async Task<IActionResult> EditAppointment(int id)
    {
        var appt = await _db.Appointments.Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
        if (appt is null) return NotFound();
        return View(appt);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> EditAppointment(Appointment model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Appointments.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Appointments");
    }

    [Authorize]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appt = await _db.Appointments.FindAsync(id);
        if (appt is null) return NotFound();
        _db.Appointments.Remove(appt);
        await _db.SaveChangesAsync();
        return RedirectToAction("Appointments");
    }

    [Authorize]
    public async Task<IActionResult> AppointmentDetails(int id)
    {
        var appt = await _db.Appointments.Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
        if (appt is null) return NotFound();
        return View(appt);
    }
}