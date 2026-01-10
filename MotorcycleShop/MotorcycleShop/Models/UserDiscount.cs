using MotorcycleShop.Models;
using System.ComponentModel.DataAnnotations;

public class UserDiscount
{
    [Key]
    public int UserDiscountId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public int DiscountId { get; set; }

    public DateTime UsedDate { get; set; } = DateTime.Now;

    // Navigation
    public ApplicationUser User { get; set; }
    public Discount Discount { get; set; }
}
