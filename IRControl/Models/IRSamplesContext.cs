using System.Data.Entity;
using IRControl.Models;

namespace IRControl.Code
{
    public class IRSamplesContext:DbContext
    {
        public IRSamplesContext()
            : base("name=Db_IRControl")
        {
        }

        public DbSet<IRSample> IRSamples { get; set; }
    }
}