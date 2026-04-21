using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("community_posts")]
public class CommunityPost
{
    [Key]
    public uint PostId { get; set; }

    [Required]
    public uint UserId { get; set; }

    [Required]
    public string ContentText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<PostMedia> PostMedia { get; set; } = new List<PostMedia>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
