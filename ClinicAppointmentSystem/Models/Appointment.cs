// Models/Appointment.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    [Required] public DateTime ScheduledAt { get; set; }
    [Required, MaxLength(100)] public string Department { get; set; } = default!;
    [MaxLength(500)] public string? Notes { get; set; }

    [MaxLength(50)] public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
}