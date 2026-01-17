using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MotorcycleShop.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        [StringLength(100)]
        public string ReportType { get; set; } // Sales, Inventory, Customer, etc.

        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Required]
        public string GeneratedByUserId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string DataJson { get; set; } // Store report data as JSON

        public string FilePath { get; set; } // Path to exported file (PDF, Excel)
        // Navigation property
        public ApplicationUser GeneratedByUser { get; set; }
    }
}