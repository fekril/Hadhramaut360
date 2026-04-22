using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("reports")]
public class Report
{
    [Key]
    public int ReportId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int PostId { get; set; }

    [Required]
    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(PostId))]
    public CommunityPost Post { get; set; } = null!;
}
