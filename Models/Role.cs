using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("roles")]
public class Role
{
    [Key]
    public uint RoleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = null!;

    [MaxLength(255)]
    public string? RoleDescription { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation property
    public ICollection<User> Users { get; set; } = new List<User>();
}
