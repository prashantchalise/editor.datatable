using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;

namespace David.UI.Models
{

	// Add profile data for application users by adding properties to the ApplicationUser class
	public class ApplicationUser : IdentityUser
	{

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
		{
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here
			return userIdentity;
		}

		// other codes
		// Add your extra profile information 
		// By Adding NotMapped attribute EF omits this and dose not puts in Identity's table

		public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid OrganizationId { get; set; }
        public int? StaffId{ get; set; }

    }
    public class ApplicationRole : IdentityRole
    {
        public string RoleGroup { get; set; }
        public Guid OrganizationId { get; set; }
    }


	
}
