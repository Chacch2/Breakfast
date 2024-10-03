using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BreakfastOrderSystem.Site.Models.ViewModels
{
    public class ReportVm
    {
        public decimal TotalSales { get; set; } // 總營業額
        public int TotalOrders { get; set; }    // 總訂單數
        public int TotalMembers { get; set; }   // 訂購會員數

        public List<SalesDataVm> SalesData { get; set; }
        public List<HotItemVm> HotItems { get; set; }
    }

    public class SalesDataVm
    {
        public int Month { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalMembers { get; set; }
    }

    public class HotItemVm
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}