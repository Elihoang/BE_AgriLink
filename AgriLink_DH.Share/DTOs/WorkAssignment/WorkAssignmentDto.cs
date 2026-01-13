using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.WorkAssignment;

public class WorkAssignmentDto
{
    public Guid Id { get; set; }
    public Guid LogId { get; set; }
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodLabel { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public string? WorkerImageUrl { get; set; }
}
