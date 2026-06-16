using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DemoExam.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column("Article")]
        public string Article { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string? Description { get; set; } = null;
        public int ManufacturerID { get; set; }
        public int SupplierID { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty; 
        public int StockQuantity { get; set; }
        public decimal Discount { get; set; }
        public string? ImagePath { get; set; } = null;

        [ForeignKey ("CategoryID")]
        public Category Category { get; set; }
        [ForeignKey ("ManufacturerID")]
        public Manufacturer Manufacturer { get; set; }
        [ForeignKey ("SupplierID")]
        public Supplier Supplier { get; set; }


        // Вычисляемые поля для UI
        [NotMapped]
        public decimal FinalPrice => Price * (1 - Discount / 100m);
        [NotMapped]
        public bool IsOutOfStock => StockQuantity == 0;
        [NotMapped]
        public bool IsDiscountedMoreThan25 => Discount > 25;

    }
}
