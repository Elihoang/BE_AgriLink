using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class WorkerValidator
{
    public void ValidateUpdate(Worker? worker, Guid id)
    {
        if (worker == null)
            throw new KeyNotFoundException($"Không tìm thấy nhân công với ID: {id}");
    }

    public void ValidateDelete(Worker? worker, Guid id)
    {
        if (worker == null)
            throw new KeyNotFoundException($"Không tìm thấy nhân công với ID: {id}");
    }
}
