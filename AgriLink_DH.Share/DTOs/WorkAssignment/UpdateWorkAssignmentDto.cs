using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.WorkAssignment;

public class UpdateWorkAssignmentDto
{
    [Required]
    public AgriLink_DH.Domain.Common.PaymentMethod PaymentMethod { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }
    
    public string Note { get; set; } = string.Empty;
}
