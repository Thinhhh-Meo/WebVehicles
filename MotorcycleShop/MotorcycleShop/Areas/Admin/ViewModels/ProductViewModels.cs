using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class CreateProductViewModel
    {
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

        public int? BrandId { get; set; }

        [Required]
        public ProductType TypeProduct { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;

        public MotorcycleCreateModel? Motorcycle { get; set; }
        public SparePartCreateModel? SparePart { get; set; }

        // For dropdown
        public List<SelectListItem>? Brands { get; set; }
    }

    public class EditProductViewModel
    {
        public int ProductId { get; set; }

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

        public int? BrandId { get; set; }

        [Required]
        public ProductType TypeProduct { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public MotorcycleEditModel? Motorcycle { get; set; }
        public SparePartEditModel? SparePart { get; set; }

        // For dropdown
        public List<SelectListItem>? Brands { get; set; }
    }

    public class MotorcycleCreateModel
    {
        
       

        
        public VehicleType? TypeVehicle { get; set; }

        
        [Range(50, 2000)]
        public int? Displacement { get; set; }

        
        [Range(0.1, 30)]
        public float? FuelCapacity { get; set; }

        
        [Range(50, 500)]
        public int? Weight { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }
    }

    public class MotorcycleEditModel
    {
        [Required]
        [StringLength(100)]
        public string? VehicleName { get; set; }

        [Required]
        public VehicleType TypeVehicle { get; set; }

        
        [Range(50, 2000)]
        public int Displacement { get; set; }

        
        [Range(0.1, 30)]
        public float FuelCapacity { get; set; }

        [Required]
        [Range(50, 500)]
        public int Weight { get; set; }

        [StringLength(50)]
        public string Color { get; set; }
    }

    public class SparePartCreateModel
    {
        
       

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? CompatibleWith { get; set; }
    }

    public class SparePartEditModel
    {
        [Required]
        [StringLength(100)]
        public string SpareName { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string CompatibleWith { get; set; }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}