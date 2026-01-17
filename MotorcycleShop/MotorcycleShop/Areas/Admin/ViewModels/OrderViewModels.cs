namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class OrderStatisticsViewModel
    {
        public dynamic MonthlyStats { get; set; }
        public dynamic StatusStats { get; set; }
        public dynamic TopProducts { get; set; }
        public int Year { get; set; }
    }
}