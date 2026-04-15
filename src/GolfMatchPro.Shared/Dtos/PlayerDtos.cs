namespace GolfMatchPro.Shared.Dtos;

public class PlayerDto
{
    public int PlayerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public decimal HandicapIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsGuest { get; set; }
}

public class CreatePlayerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public decimal HandicapIndex { get; set; }
    public bool IsGuest { get; set; }
}

public class UpdatePlayerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public decimal HandicapIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsGuest { get; set; }
}
