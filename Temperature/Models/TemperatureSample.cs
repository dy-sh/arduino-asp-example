using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace Temperature.Models
{
    public class TemperatureSample
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public float? Temperature { get; set; }
        public float? Humidity { get; set; }

    }
}