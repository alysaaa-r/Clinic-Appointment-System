// Models/BaseEntity.cs
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string CreatedBy { get; set; } = "system";

    public DateTime CreatedAt { get; set; } // DB default CURRENT_TIMESTAMP
    public DateTime? UpdatedAt { get; set; } // DB auto update ON UPDATE CURRENT_TIMESTAMP
}