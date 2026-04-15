using System.ComponentModel.DataAnnotations;

namespace GolfMatchPro.Data.Entities;

public class Player
{
    public int PlayerId { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Nickname { get; set; }

    public decimal HandicapIndex { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsGuest { get; set; }

    [MaxLength(200)]
    public string? EntraUserId { get; set; }
}
