using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon.BeesPortal.Web.Models.Portal
{
    public class Hive
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ApiaryId { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string SigfoxId { get; set; }
    }
}