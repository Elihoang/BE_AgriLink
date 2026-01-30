using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Article;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AgriLink_DH.Core.Helpers;

/// <summary>
/// Helper class cho Article domain - chứa các utility methods
/// </summary>
public static class ArticleHelper
{
    /// <summary>
    /// Chuyển tiêu đề tiếng Việt thành slug SEO-friendly
    /// VD: "Kỹ thuật trồng lúa" -> "ky-thuat-trong-lua"
    /// </summary>
    public static string GenerateSlug(string title)
    {
        // Vietnamese to ASCII conversion map
        var vietnameseMap = new Dictionary<char, string>
        {
            {'à', "a"}, {'á', "a"}, {'ả', "a"}, {'ã', "a"}, {'ạ', "a"},
            {'â', "a"}, {'ầ', "a"}, {'ấ', "a"}, {'ẩ', "a"}, {'ẫ', "a"}, {'ậ', "a"},
            {'ă', "a"}, {'ằ', "a"}, {'ắ', "a"}, {'ẳ', "a"}, {'ẵ', "a"}, {'ặ', "a"},
            {'è', "e"}, {'é', "e"}, {'ẻ', "e"}, {'ẽ', "e"}, {'ẹ', "e"},
            {'ê', "e"}, {'ề', "e"}, {'ế', "e"}, {'ể', "e"}, {'ễ', "e"}, {'ệ', "e"},
            {'ì', "i"}, {'í', "i"}, {'ỉ', "i"}, {'ĩ', "i"}, {'ị', "i"},
            {'ò', "o"}, {'ó', "o"}, {'ỏ', "o"}, {'õ', "o"}, {'ọ', "o"},
            {'ô', "o"}, {'ồ', "o"}, {'ố', "o"}, {'ổ', "o"}, {'ỗ', "o"}, {'ộ', "o"},
            {'ơ', "o"}, {'ờ', "o"}, {'ớ', "o"}, {'ở', "o"}, {'ỡ', "o"}, {'ợ', "o"},
            {'ù', "u"}, {'ú', "u"}, {'ủ', "u"}, {'ũ', "u"}, {'ụ', "u"},
            {'ư', "u"}, {'ừ', "u"}, {'ứ', "u"}, {'ử', "u"}, {'ữ', "u"}, {'ự', "u"},
            {'ỳ', "y"}, {'ý', "y"}, {'ỷ', "y"}, {'ỹ', "y"}, {'ỵ', "y"},
            {'đ', "d"},
            {'À', "a"}, {'Á', "a"}, {'Ả', "a"}, {'Ã', "a"}, {'Ạ', "a"},
            {'Â', "a"}, {'Ầ', "a"}, {'Ấ', "a"}, {'Ẩ', "a"}, {'Ẫ', "a"}, {'Ậ', "a"},
            {'Ă', "a"}, {'Ằ', "a"}, {'Ắ', "a"}, {'Ẳ', "a"}, {'Ẵ', "a"}, {'Ặ', "a"},
            {'È', "e"}, {'É', "e"}, {'Ẻ', "e"}, {'Ẽ', "e"}, {'Ẹ', "e"},
            {'Ê', "e"}, {'Ề', "e"}, {'Ế', "e"}, {'Ể', "e"}, {'Ễ', "e"}, {'Ệ', "e"},
            {'Ì', "i"}, {'Í', "i"}, {'Ỉ', "i"}, {'Ĩ', "i"}, {'Ị', "i"},
            {'Ò', "o"}, {'Ó', "o"}, {'Ỏ', "o"}, {'Õ', "o"}, {'Ọ', "o"},
            {'Ô', "o"}, {'Ồ', "o"}, {'Ố', "o"}, {'Ổ', "o"}, {'Ỗ', "o"}, {'Ộ', "o"},
            {'Ơ', "o"}, {'Ờ', "o"}, {'Ớ', "o"}, {'Ở', "o"}, {'Ỡ', "o"}, {'Ợ', "o"},
            {'Ù', "u"}, {'Ú', "u"}, {'Ủ', "u"}, {'Ũ', "u"}, {'Ụ', "u"},
            {'Ư', "u"}, {'Ừ', "u"}, {'Ứ', "u"}, {'Ử', "u"}, {'Ữ', "u"}, {'Ự', "u"},
            {'Ỳ', "y"}, {'Ý', "y"}, {'Ỷ', "y"}, {'Ỹ', "y"}, {'Ỵ', "y"},
            {'Đ', "d"}
        };

        var sb = new StringBuilder();
        foreach (var c in title.ToLowerInvariant())
        {
            if (vietnameseMap.TryGetValue(c, out var replacement))
            {
                sb.Append(replacement);
            }
            else if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                sb.Append('-');
            }
        }

        var slug = sb.ToString();
        slug = Regex.Replace(slug, @"-+", "-"); // Replace multiple dashes with single dash
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Tính thời gian relative bằng tiếng Việt
    /// VD: "một ngày trước", "3 tuần trước", "5 tháng trước"
    /// </summary>
    public static string GetTimeAgo(DateTime? publishedAt)
    {
        if (!publishedAt.HasValue) return string.Empty;

        var timeSpan = DateTime.UtcNow - publishedAt.Value;

        if (timeSpan.TotalDays >= 30)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"{months} tháng trước";
        }
        if (timeSpan.TotalDays >= 7)
        {
            var weeks = (int)(timeSpan.TotalDays / 7);
            return $"{weeks} tuần trước";
        }
        if (timeSpan.TotalDays >= 1)
        {
            var days = (int)timeSpan.TotalDays;
            return $"{days} ngày trước";
        }
        if (timeSpan.TotalHours >= 1)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"{hours} giờ trước";
        }
        if (timeSpan.TotalMinutes >= 1)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} phút trước";
        }

        return "vừa xong";
    }
}

