using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? Industry { get; set; }
        public string? AvatarUrl { get; set; }
        [NotMapped]
        public IFormFile? Avatar { get; set; }
        public string? Description { get; set; }
        public string? Requiered1 { get; set; }
        public string? Requiered2 { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public int? LowestPrice { get; set; }
        public int? HighestPrice { get; set; }
        [ForeignKey("Users")]
        public string UserId { get; set; }
        public virtual ApplicationUser Users { get; set; }
    }
}
