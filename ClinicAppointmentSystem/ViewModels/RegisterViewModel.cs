// ViewModels/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels;

public class RegisterViewModel
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = default!;
    [Required, MaxLength(100)] public string LastName { get; set; } = default!;
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = default!;
    [Required] public string Role { get; set; } = "User"; // "User" or "Admin"
}