using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Introduction { get; set; }
        public string? CVUrl { get; set; }
        public int? JobId { get; set; }
        [NotMapped]
        public IFormFile? CurriculumVitae { get; set; }
        public string? Status { get; set; }
        [ForeignKey("Users")]
        public string UserId { get; set; }
        public virtual ApplicationUser Users { get; set; }
    }
}
