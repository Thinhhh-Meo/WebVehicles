using MotorcycleShop.Models;
using System;
using System.Collections.Generic;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class SalesReportViewModel
    {
        public List<Order> Orders { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public OrderStatus? SelectedStatus { get; set; }
    }
}
