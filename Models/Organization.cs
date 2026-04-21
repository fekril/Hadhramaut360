using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("organizations")]
public class Organization
{
    [Key]
    public uint UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string OrgNameAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string OrgNameEn { get; set; } = null!;

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    [Required]
    [MaxLength(150)]
    public string AddressAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string AddressEn { get; set; } = null!;

    [MaxLength(500)]
    public string? MapUrl { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
