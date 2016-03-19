using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon.BeesPortal.Web.Models.Portal
{
    public class Notification
    {
        [Required]
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string SigfoxId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        public string Severity { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(128)]
        public string Text { get; set; }

        public bool Viewed { get; set; }

        [Required]
        public DateTime DateTime { get; set; }
    }
}