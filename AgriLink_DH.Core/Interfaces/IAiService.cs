using System.Threading.Tasks;
using AgriLink_DH.Share.DTOs.Ai;

namespace AgriLink_DH.Core.Interfaces;

public interface IAiService
{
    Task<GeminiQueryResponseDto> AskQuestionAsync(GeminiQueryRequestDto request);
}
