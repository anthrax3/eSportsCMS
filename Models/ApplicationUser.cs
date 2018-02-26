using Microsoft.AspNetCore.Identity;

namespace WebDevEsports.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
    }
}
