using Microsoft.AspNetCore.Identity;

namespace VgcCollege.MVC.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
