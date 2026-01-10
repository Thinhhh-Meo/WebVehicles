namespace MotorcycleShop.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public ProductType TypeProduct { get; set; }
    }
}