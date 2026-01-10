using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleShop.Models
{
    public enum VehicleType
    {
        Sport = 1,
        Cruiser = 2,
        Scooter = 3,
        Adventure = 4,
        Naked = 5
    }

    public class Motorcycle
    {
        [Key]
        public int MotorcycleId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string VehicleName { get; set; }

        [Required]
        public VehicleType TypeVehicle { get; set; }

        [Required]
        [Range(50, 2000)]
        public int Displacement { get; set; } // Dung tích xi lanh (cc)

        [Required]
        [Range(0.1, 30)]
        public float FuelCapacity { get; set; } // Dung tích bình xăng (lít)

        [Required]
        [Range(50, 500)]
        public int Weight { get; set; } // Cân nặng (kg)

        [StringLength(50)]
        public string Color { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}