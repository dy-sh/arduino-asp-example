using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRControl.Models
{
    public class IRSample
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int Length { get; set; }
        public int Value { get; set; }
        public int[] Raw { get; set; }
    }
}