// ViewModels/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels;

public class ForgotPasswordViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
}