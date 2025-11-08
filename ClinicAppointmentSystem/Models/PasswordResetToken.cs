// Models/PasswordResetToken.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class PasswordResetToken : BaseEntity
{
    [Required, MaxLength(255)] public string Email { get; set; } = default!;
    [Required, MaxLength(100)] public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
}