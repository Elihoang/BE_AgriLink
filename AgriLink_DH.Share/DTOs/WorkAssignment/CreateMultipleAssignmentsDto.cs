using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.WorkAssignment;

public class CreateMultipleAssignmentsDto
{
    [Required]
    public Guid LogId { get; set; }
    
    [Required]
    public List<CreateWorkAssignmentDto> Assignments { get; set; } = new();
}
