namespace AgriLink_DH.Domain.Models.Base;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
