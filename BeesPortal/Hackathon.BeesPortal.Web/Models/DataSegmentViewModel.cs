using System.Collections.Generic;
using Hackathon.BeesPortal.Web.Models.Portal;

namespace Hackathon.BeesPortal.Web.Models
{
    public class DataSegmentViewModel
    {
        public List<Apiary> Apiaries { get; set; }
        public List<Hive> Hives { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<DataSegment> DataSegments { get; set; }
    }
}