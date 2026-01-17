using MotorcycleShop.Models;
using MotorcycleShop.Models.Admin;
using System.Collections.Generic;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public decimal MonthlyRevenue { get; set; }
        public int MonthlyOrders { get; set; }

        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<DailyOrder> DailyOrders { get; set; } = new List<DailyOrder>();
    }

    public class DailyOrder
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }
}