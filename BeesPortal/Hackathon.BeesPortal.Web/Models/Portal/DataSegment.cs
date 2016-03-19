using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon.BeesPortal.Web.Models.Portal
{
    public class DataSegment
    {
        [Required]
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(5)]
        public string SigfoxId { get; set; }

        public float? Temperature { get; set; }
        public float? Humidity { get; set; }

        [Required]
        public DateTime DateTime { get; set; }
    }
}