using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TravelAppApi.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }
        [Column("userName")]
        public string? Username { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("passwordHash")]
        public string? PasswordHash { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }
        [Column("role")]
        public string? Role { get; set; }
        [Column("avatarUrl")]
        public string? AvatarUrl { get; set; }

        public ICollection<Trip>? Trips { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}
