using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class BrandCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [DataType(DataType.Date)]
        public DateTime EstablishedYear { get; set; } = new DateTime(2000, 1, 1);

        public IFormFile? LogoFile { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BrandEditViewModel
    {
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [DataType(DataType.Date)]
        public DateTime EstablishedYear { get; set; }

        public IFormFile? LogoFile { get; set; }
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; }
    }
}