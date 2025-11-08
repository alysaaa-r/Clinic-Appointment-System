// ViewModels/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels;

public class ResetPasswordViewModel
{
    [Required] public string Token { get; set; } = default!;
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required, DataType(DataType.Password)] public string NewPassword { get; set; } = default!;
}