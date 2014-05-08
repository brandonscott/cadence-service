using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CadenceService.Models
{
    public class UserAction
    {
        public string action { get; set; }
        public int timestamp { get; set; }
        public string uuid { get; set; }
        public int occupancy { get; set; }

        [ScriptIgnore]
        public DateTime date {
            get
            {
                System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds( timestamp ).ToLocalTime();
                return dtDateTime;
            }
        }
    }
}