using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadenceService.Models
{
    public class PresenceList
    {
        public int status { get; set; }
        public string message { get; set; }
        public string service { get; set; }
        public List<string> uuids { get; set; }
        public int occupancy { get; set; }
    }
}