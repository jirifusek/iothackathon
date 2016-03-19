using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon.BeesPortal.Web.Models.Portal
{
    public class Apiary
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(256)]
        public string Label { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(128)]
        public string Location { get; set; }
    }
}