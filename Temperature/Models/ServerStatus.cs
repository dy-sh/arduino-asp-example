using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Temperature.Models
{
    public class ServerStatus
    {
        public int TimeOnline { get; set; }
        public int TotalRequests { get; set; }
    }
}