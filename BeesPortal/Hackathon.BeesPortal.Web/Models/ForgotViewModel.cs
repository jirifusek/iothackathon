using System.ComponentModel.DataAnnotations;

namespace Hackathon.BeesPortal.Web.Models
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}