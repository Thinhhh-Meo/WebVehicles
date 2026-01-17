using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class PromotionCreateViewModel
    {
        [Required]
        [StringLength(200)]
        public string Condition { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        public bool IsActive { get; set; } = true;

        [Range(0, 100)]
        public int DisplayOrder { get; set; }

        public int? DiscountId { get; set; }
    }

    public class PromotionEditViewModel
    {
        public int PromotionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Condition { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImagePath { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        [Range(0, 100)]
        public int DisplayOrder { get; set; }

        public int? DiscountId { get; set; }
    }
}