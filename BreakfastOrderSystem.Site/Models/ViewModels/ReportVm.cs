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
    }
}