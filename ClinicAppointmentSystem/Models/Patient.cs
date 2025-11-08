// Models/Patient.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class Patient : BaseEntity
{
    [Required, MaxLength(150)] public string FullName { get; set; } = default!;
    [MaxLength(20)] public string? Phone { get; set; }
    [EmailAddress, MaxLength(255)] public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Optional link to a registered user (self-appointment)
    public int? UserId { get; set; }
    public User? User { get; set; }
}