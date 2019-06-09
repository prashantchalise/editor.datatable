using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace David.UI.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext()  : base("RolpoContext")
		{

		}

		public static ApplicationDbContext Create()
		{
			return new ApplicationDbContext();
		}

	}
}
