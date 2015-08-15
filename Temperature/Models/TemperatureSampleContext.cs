using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Temperature.Models
{
    public class TemperatureSampleContext:DbContext
    {
        public TemperatureSampleContext()
            : base("name=DbConnection")
        {
        }

        public DbSet<TemperatureSample> TemperatureSamples { get; set; }
    }
}