using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("users")]
public class User
{
    [Key]
    public uint UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [Required]
    public uint RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    public Organization? Organization { get; set; }

    public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
