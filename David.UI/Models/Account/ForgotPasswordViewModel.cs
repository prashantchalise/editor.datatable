using System.ComponentModel.DataAnnotations;

namespace David.UI.Models
{
	public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
