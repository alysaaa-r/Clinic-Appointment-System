// Models/Admin.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class Admin : BaseEntity
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = default!;
    [Required, MaxLength(100)] public string LastName { get; set; } = default!;
    [Required, EmailAddress, MaxLength(255)] public string Email { get; set; } = default!;
    [Required] public string PasswordHash { get; set; } = default!;
    [Required] public string PasswordSalt { get; set; } = default!;
    public bool IsApproved { get; set; } = false;
    public bool IsMainAdmin { get; set; } = false;
}