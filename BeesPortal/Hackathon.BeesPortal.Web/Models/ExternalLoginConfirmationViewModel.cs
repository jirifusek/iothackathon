﻿using System.ComponentModel.DataAnnotations;

namespace Hackathon.BeesPortal.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}