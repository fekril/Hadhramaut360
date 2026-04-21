using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("post_media")]
public class PostMedia
{
    [Key]
    public uint MediaId { get; set; }

    [Required]
    public uint PostId { get; set; }

    [Required]
    [MaxLength(500)]
    public string MediaUrl { get; set; } = null!;

    [Required]
    public MediaType MediaType { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(PostId))]
    public CommunityPost Post { get; set; } = null!;
}
