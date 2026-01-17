
using System.ComponentModel.DataAnnotations;
namespace MotorcycleShop.Models.Admin
{
    public class AdminLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public string AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(100)]
        public string Entity { get; set; }

        public int? EntityId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string IPAddress { get; set; }

        // Navigation
        public ApplicationUser Admin { get; set; }
    }
}
