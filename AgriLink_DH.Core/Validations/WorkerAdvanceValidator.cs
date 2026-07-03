using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class WorkerAdvanceValidator
{
    public void ValidateCreate(Worker? worker, CropSeason? season)
    {
        if (worker == null)
            throw new InvalidOperationException("Không tìm thấy nhân công");

        if (season == null)
            throw new InvalidOperationException("Không tìm thấy vụ mùa");
    }

    public void ValidateDelete(WorkerAdvance? advance)
    {
        if (advance == null)
            throw new KeyNotFoundException("Không tìm thấy khoản ứng lương");

        if (advance.IsDeducted)
            throw new InvalidOperationException("Không thể xóa khoản ứng đã được trừ vào lương!");
    }
}
