using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("users")]
public class User : IdentityUser<int>
{
    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Organization? Organization { get; set; }

    public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
