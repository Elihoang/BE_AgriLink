using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Chi tiết Bao - Lines (Cân từng bao tại rẫy)
/// </summary>
[Table("harvest_bag_details")]
public class HarvestBagDetail
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("session_id")]
    [Required]
    public Guid SessionId { get; set; }

    [Column("bag_index")]
    public int BagIndex { get; set; } // STT: 1, 2, 3

    [Column("gross_weight")]
    [Precision(10, 2)]
    public decimal GrossWeight { get; set; } // Cân cả bì: 50.5

    [Column("deduction")]
    [Precision(10, 2)]
    public decimal Deduction { get; set; } = 0.5m; // Trừ bì: 0.5

    [Column("net_weight")]
    [Precision(10, 2)]
    public decimal NetWeight { get; set; } // = Gross - Deduction (Lưu cứng hoặc Generated Column)

    // Navigation Properties
    [ForeignKey(nameof(SessionId))]
    public virtual HarvestSession HarvestSession { get; set; } = null!;
}
