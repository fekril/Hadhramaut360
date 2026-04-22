using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("events")]
public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required]
    public int OrgUserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleAr { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = null!;

    [Required]
    public string DescriptionAr { get; set; } = null!;

    [Required]
    public string DescriptionEn { get; set; } = null!;

    [MaxLength(500)]
    public string? MapUrl { get; set; }

    [Required]
    [MaxLength(150)]
    public string AddressAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string AddressEn { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string FormUrl { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OrgUserId))]
    public Organization Organization { get; set; } = null!;

    public ICollection<EventMedia> EventMedia { get; set; } = new List<EventMedia>();
}
