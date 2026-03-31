namespace AgriLink_DH.Share.DTOs.Disease;

public class DiagnoseResultDto
{
    public string Disease      { get; set; } = string.Empty; // tên bệnh (English class name)
    public string DiseaseLabel { get; set; } = string.Empty; // tên tiếng Việt
    public double Confidence   { get; set; }                 // 0.0 → 1.0
    public string ConfidenceText => $"{Confidence:P1}";      // "95.5%"
    public string Advice       { get; set; } = string.Empty; // gợi ý xử lý
}
