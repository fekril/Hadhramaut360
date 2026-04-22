using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("event_media")]
public class EventMedia
{
    [Key]
    public int MediaId { get; set; }

    [Required]
    public int EventId { get; set; }

    [Required]
    [MaxLength(500)]
    public string MediaUrl { get; set; } = null!;

    [Required]
    public MediaType MediaType { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;
}
