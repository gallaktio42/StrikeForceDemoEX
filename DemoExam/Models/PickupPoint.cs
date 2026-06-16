using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DemoExam.Models
{
    public class PickupPoint
    {
        [Key]
        public int PointID { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
