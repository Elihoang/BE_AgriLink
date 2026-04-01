namespace AgriLink_DH.Share.DTOs.Ai;

public class GeminiQueryResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double UsageTokens { get; set; }
}
