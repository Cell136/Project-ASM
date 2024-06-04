using Microsoft.AspNetCore.Identity;

namespace Project.Models
{
    public class ApplicationRole: IdentityRole
    {

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
