namespace AgriLink_DH.Share.DTOs.User;

public class UserLoginLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public DateTime LoginTime { get; set; }
    public bool IsSuccess { get; set; }
    public string ActionType { get; set; } = string.Empty;
}
