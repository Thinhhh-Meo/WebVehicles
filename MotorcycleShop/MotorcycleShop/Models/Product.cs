using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleShop.Models
{
    public enum ProductType
    {
        Motorcycle = 1,
        SparePart = 2
    }

    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public ProductType TypeProduct { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string BrandName { get; set; }

        public int? BrandId {  get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;


        // Navigation properties
        [ForeignKey("BrandId")]
        public Brand? Brand { get; set; }
        public Motorcycle? Motorcycle { get; set; }
        public SparePart? SparePart { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}