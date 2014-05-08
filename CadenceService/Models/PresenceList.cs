using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadenceService.Models
{
    public class Uuid
    {
        public string uuid { get; set; }
    }

    public class PresenceList
    {
        public int status { get; set; }
        public string message { get; set; }
        public string service { get; set; }
        public List<Uuid> uuids { get; set; }
        public int occupancy { get; set; }
    }
}