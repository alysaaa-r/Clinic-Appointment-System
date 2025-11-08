// Controllers/UserDashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Controllers;

[Authorize(Policy = "UserOnly")]
public class UserDashboardController : Controller
{
    private readonly AppDbContext _db;
    public UserDashboardController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        var appointments = new List<Appointment>();
        if (user is not null)
        {
            var patientIds = await _db.Patients.Where(p => p.UserId == user.Id).Select(p => p.Id).ToListAsync();
            appointments = await _db.Appointments.Include(a => a.Patient)
                .Where(a => patientIds.Contains(a.PatientId))
                .OrderByDescending(a => a.ScheduledAt)
                .ToListAsync();
        }
        ViewBag.Appointments = appointments;
        return View();
    }

    [HttpGet]
    public IActionResult Book()
    {
        return View(new AppointmentBookingViewModel { ScheduledAt = DateTime.Today.AddDays(1) });
    }

    [HttpPost]
    public async Task<IActionResult> Book(AppointmentBookingViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized();

        // Create or reuse patient record for this user
        var fullName = $"{user.FirstName} {user.LastName}";
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (patient is null)
        {
            patient = new Patient
            {
                FullName = fullName,
                Email = user.Email,
                UserId = user.Id,
                CreatedBy = user.Email
            };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
        }

        var appt = new Appointment
        {
            PatientId = patient.Id,
            ScheduledAt = vm.ScheduledAt,
            Department = vm.Department,
            Notes = vm.Notes,
            Status = "Pending",
            CreatedBy = user.Email
        };
        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Appointment requested. You will receive confirmation.";
        return RedirectToAction("Index");
    }
}