using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.WorkAssignment;

public class CreateWorkAssignmentDto
{
    [Required]
    public Guid LogId { get; set; }

    [Required]
    public Guid WorkerId { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Daily;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }
}
