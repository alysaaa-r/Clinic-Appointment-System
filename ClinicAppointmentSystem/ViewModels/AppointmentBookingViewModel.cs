// ViewModels/AppointmentBookingViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class AppointmentBookingViewModel
{
    [Required] public DateTime ScheduledAt { get; set; }
    [Required, MaxLength(100)] public string Department { get; set; } = default!;
    [MaxLength(500)] public string? Notes { get; set; }
}