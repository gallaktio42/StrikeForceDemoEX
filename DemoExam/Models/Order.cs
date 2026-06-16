using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoExam.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

        public int StatusID { get; set; }
        public int PointID { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? IssueDate { get; set; }

        [ForeignKey("StatusID")]
        public OrderStatus Status { get; set; }

        [ForeignKey("PointID")]
        public PickupPoint PickupPoint { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
