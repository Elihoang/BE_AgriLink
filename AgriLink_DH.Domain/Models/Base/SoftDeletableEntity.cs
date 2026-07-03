namespace AgriLink_DH.Domain.Models.Base;

public abstract class SoftDeletableEntity : BaseEntity
{
    public bool IsDeleted { get; protected set; } = false;
    public DateTime? DeletedAt { get; protected set; }

    public virtual void SoftDelete()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }

    public virtual void Restore()
    {
        if (IsDeleted)
        {
            IsDeleted = false;
            DeletedAt = null;
        }
    }
}
