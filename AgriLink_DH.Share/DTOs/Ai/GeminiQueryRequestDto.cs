namespace AgriLink_DH.Share.DTOs.Ai;

public class GeminiQueryRequestDto
{
    public string Question { get; set; } = string.Empty;
    public string? Context { get; set; }
}
