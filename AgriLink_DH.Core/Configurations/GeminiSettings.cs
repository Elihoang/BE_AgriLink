namespace AgriLink_DH.Core.Configurations;

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public int MaxTokens { get; set; } = 800;
}
