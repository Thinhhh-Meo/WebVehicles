using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleShop.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [BindNever]
        public string UserId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [BindNever]
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }


        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}