using ClosedXML.Excel;
using AgriLink_DH.Share.DTOs.MarketPrice;

namespace AgriLink_DH.Core.Helpers;

/// <summary>
/// Kết quả parse file Excel giá thị trường
/// </summary>
public class ExcelParseResult
{
    /// <summary>Các dòng parse thành công → sẵn sàng gọi UpdatePriceAsync</summary>
    public List<UpdateMarketPriceRequest> ValidRows { get; set; } = new();

    /// <summary>Các dòng lỗi với mô tả lỗi</summary>
    public List<ExcelRowError> Errors { get; set; } = new();

    public bool HasErrors => Errors.Count > 0;
    public int SuccessCount => ValidRows.Count;
}

public class ExcelRowError
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Helper đọc file Excel giá nông sản (ClosedXML - MIT License).
///
/// ─── Format file CÀ PHÊ (import-coffee.xlsx) ───────────────────────────
/// Row 1 (Header): Region | RegionCode | Price | RecordedDate | Source
/// Row 2+:         Đắk Lắk | DAK_LAK | 65200 | 2026-02-24 | giacaphe.com
///
/// ─── Format file HỒ TIÊU (import-pepper.xlsx) ──────────────────────────
/// Row 1 (Header): Price | RecordedDate | Source
/// Row 2+:         93500 | 2026-02-24 | giacaphe.com
/// (Hồ tiêu 1 giá toàn quốc — không cần Region)
///
/// NOTE: Cột "Change" đã bị bỏ — backend tự tính dựa trên giá ngày trước đó.
/// </summary>
public static class ExcelPriceHelper
{
    // ProductId KHÔNG hardcode — do caller truyền vào sau khi lookup từ DB
    // (GetProductIdByCodeAsync("CF_ROBUSTA") / GetProductIdByCodeAsync("PEPPER"))

    // ─── CÀ PHÊ ──────────────────────────────────────────────────────────────────
    /// <summary>
    /// Parse file Excel giá cà phê theo khu vực tỉnh.
    /// Cột bắt buộc: Region, RegionCode, Price
    /// Cột tuỳ chọn: Change, RecordedDate, Source
    /// </summary>
    public static ExcelParseResult ParseCoffeeExcel(Stream fileStream, Guid productId)
    {
        var result = new ExcelParseResult();

        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheets.First();

        // Đọc header row 1 để biết thứ tự cột (case-insensitive)
        var headers = ReadHeaders(ws);

        // Validate bắt buộc
        var requiredCols = new[] { "region", "regioncode", "price" };
        foreach (var col in requiredCols)
        {
            if (!headers.ContainsKey(col))
            {
                result.Errors.Add(new ExcelRowError
                {
                    RowNumber = 1,
                    Message = $"Thiếu cột bắt buộc: '{col}'. File phải có: Region | RegionCode | Price | [RecordedDate] | [Source]"
                });
                return result; // Dừng sớm nếu thiếu cột
            }
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = 2; row <= lastRow; row++)
        {
            try
            {
                var region     = GetCellString(ws, row, headers, "region");
                var regionCode = GetCellString(ws, row, headers, "regioncode");
                var priceRaw   = GetCellString(ws, row, headers, "price");

                // Bỏ qua dòng trống
                if (string.IsNullOrWhiteSpace(region) && string.IsNullOrWhiteSpace(priceRaw))
                    continue;

                // Validate bắt buộc
                if (string.IsNullOrWhiteSpace(region))
                {
                    result.Errors.Add(new ExcelRowError { RowNumber = row, Message = "Cột Region không được để trống." });
                    continue;
                }
                if (string.IsNullOrWhiteSpace(regionCode))
                {
                    result.Errors.Add(new ExcelRowError { RowNumber = row, Message = "Cột RegionCode không được để trống." });
                    continue;
                }
                if (!TryParseVND(priceRaw, out var price) || price <= 0)
                {
                    result.Errors.Add(new ExcelRowError { RowNumber = row, Message = $"Giá không hợp lệ: '{priceRaw}'. Phải là số VND dương (VD: 65200)." });
                    continue;
                }

                // Cột tuỳ chọn (Change bỏ — backend tự tính)
                var recordedDate = TryParseDate(GetCellString(ws, row, headers, "recordeddate"));
                var source       = GetCellString(ws, row, headers, "source");

                result.ValidRows.Add(new UpdateMarketPriceRequest
                {
                    ProductId    = productId,   // ← từ DB, không hardcode
                    Region       = region.Trim(),
                    RegionCode   = regionCode.Trim().ToUpper(),
                    Price        = price,
                    Change       = 0,           // backend tự tính từ giá ngày trước
                    RecordedDate = recordedDate,
                    Source       = string.IsNullOrWhiteSpace(source) ? "Excel Import" : source.Trim(),
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ExcelRowError { RowNumber = row, Message = $"Lỗi không xác định: {ex.Message}" });
            }
        }

        return result;
    }

    // ─── HỒ TIÊU ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Parse file Excel giá hồ tiêu toàn quốc (không có Region).
    /// Cột bắt buộc: Price
    /// Cột tuỳ chọn: Change, RecordedDate, Source
    /// </summary>
    public static ExcelParseResult ParsePepperExcel(Stream fileStream, Guid productId)
    {
        var result = new ExcelParseResult();

        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheets.First();

        var headers = ReadHeaders(ws);

        if (!headers.ContainsKey("price"))
        {
            result.Errors.Add(new ExcelRowError
            {
                RowNumber = 1,
                Message = "Thiếu cột 'Price'. File phải có: Price | [RecordedDate] | [Source]"
            });
            return result;
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = 2; row <= lastRow; row++)
        {
            try
            {
                var priceRaw = GetCellString(ws, row, headers, "price");

                if (string.IsNullOrWhiteSpace(priceRaw))
                    continue;

                if (!TryParseVND(priceRaw, out var price) || price <= 0)
                {
                    result.Errors.Add(new ExcelRowError { RowNumber = row, Message = $"Giá không hợp lệ: '{priceRaw}'." });
                    continue;
                }

                // Cột tuỳ chọn (Change bỏ — backend tự tính)
                var recordedDate = TryParseDate(GetCellString(ws, row, headers, "recordeddate"));
                var source       = GetCellString(ws, row, headers, "source");

                result.ValidRows.Add(new UpdateMarketPriceRequest
                {
                    ProductId    = productId,   // ← từ DB, không hardcode
                    Region       = "Toàn quốc",
                    RegionCode   = null,          // null = toàn quốc
                    Price        = price,
                    Change       = 0,             // backend tự tính từ giá ngày trước
                    RecordedDate = recordedDate,
                    Source       = string.IsNullOrWhiteSpace(source) ? "Excel Import" : source.Trim(),
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ExcelRowError { RowNumber = row, Message = $"Lỗi: {ex.Message}" });
            }
        }

        return result;
    }

    // ─── Tạo file Excel mẫu để Admin tải về ──────────────────────────────────────
    /// <summary>Tạo file Excel mẫu cho Cà phê (template download)</summary>
    public static byte[] GenerateCoffeeTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Coffee Prices");

        // Header — bỏ cột "Change", backend tự tính
        var headers = new[] { "Region", "RegionCode", "Price", "RecordedDate", "Source" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#16a34a");
            cell.Style.Font.FontColor       = XLColor.White;
        }

        // Ghi chú tự động tính Change
        var noteCell = ws.Cell(1, headers.Length + 1);
        noteCell.Value = "[Change được tính tự động — không cần điền]";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.Gray;

        // Ví dụ data (không có cột Change)
        var examples = new[]
        {
            new object[] { "Đắk Lắk",    "DAK_LAK",    65200, DateTime.Today.ToString("yyyy-MM-dd"), "giacaphe.com" },
            new object[] { "Gia Lai",     "GIA_LAI",    64800, DateTime.Today.ToString("yyyy-MM-dd"), "giacaphe.com" },
            new object[] { "Lâm Đồng",   "LAM_DONG",   64500, DateTime.Today.ToString("yyyy-MM-dd"), "giacaphe.com" },
            new object[] { "Đắk Nông",   "DAK_NONG",   65000, DateTime.Today.ToString("yyyy-MM-dd"), "giacaphe.com" },
            new object[] { "Bình Phước",  "BINH_PHUOC", 64300, DateTime.Today.ToString("yyyy-MM-dd"), "" },
            new object[] { "Bà Rịa",     "BA_RIA",     64600, DateTime.Today.ToString("yyyy-MM-dd"), "" },
        };
        for (int r = 0; r < examples.Length; r++)
        {
            for (int c = 0; c < examples[r].Length; c++)
                ws.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(examples[r][c]);
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>Tạo file Excel mẫu cho Hồ Tiêu</summary>
    public static byte[] GeneratePepperTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Pepper Prices");

        // Header — bỏ cột "Change", backend tự tính
        var headers = new[] { "Price", "RecordedDate", "Source" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d9488");
            cell.Style.Font.FontColor       = XLColor.White;
        }

        // Ghi chú tự động tính Change
        var noteCell = ws.Cell(1, headers.Length + 1);
        noteCell.Value = "[Change được tính tự động — không cần điền]";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.Gray;

        // Ví dụ: mỗi ngày 1 dòng giá toàn quốc (không có cột Change)
        var examples = new[]
        {
            new object[] { 93500, DateTime.Today.ToString("yyyy-MM-dd"),             "giacaphe.com" },
            new object[] { 93300, DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), "giacaphe.com" },
            new object[] { 93400, DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), "" },
        };
        for (int r = 0; r < examples.Length; r++)
        {
            for (int c = 0; c < examples[r].Length; c++)
                ws.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(examples[r][c]);
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ─── Private helpers ──────────────────────────────────────────────────────────

    /// <summary>Đọc row 1 thành dict: header_lowercase → column_index (1-based)</summary>
    private static Dictionary<string, int> ReadHeaders(IXLWorksheet ws)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastCol = ws.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;

        for (int col = 1; col <= lastCol; col++)
        {
            var val = ws.Cell(1, col).GetString().Trim().ToLower().Replace(" ", "");
            if (!string.IsNullOrEmpty(val) && !dict.ContainsKey(val))
                dict[val] = col;
        }
        return dict;
    }

    /// <summary>Lấy giá trị cell theo tên cột. Trả về "" nếu cột không tồn tại.</summary>
    private static string GetCellString(IXLWorksheet ws, int row, Dictionary<string, int> headers, string colKey)
    {
        if (!headers.TryGetValue(colKey, out var col)) return string.Empty;
        return ws.Cell(row, col).GetString().Trim();
    }

    /// <summary>
    /// Parse VND từ chuỗi — chấp nhận: "65200", "65.200", "65,200"
    /// Bỏ qua dấu chấm và phẩy vì VN dùng chúng làm thousand separator.
    /// </summary>
    private static bool TryParseVND(string raw, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var clean = raw.Replace(".", "").Replace(",", "").Trim();
        return decimal.TryParse(clean, out value);
    }

    /// <summary>Parse ngày từ chuỗi, trả null nếu không parse được.</summary>
    private static DateTime? TryParseDate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (DateTime.TryParse(raw, out var dt)) return dt.Date;
        return null;
    }
}
