// ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = default!;
}