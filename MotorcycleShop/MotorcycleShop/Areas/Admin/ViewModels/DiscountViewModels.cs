using MotorcycleShop.Models;
using System.Collections.Generic;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class UsageReportViewModel
    {
        public Discount Discount { get; set; }
        public List<UserDiscount> UsageData { get; set; }
        public int TotalUses { get; set; }
        public int UniqueUsers { get; set; }
    }
}