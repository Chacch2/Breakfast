using BreakfastOrderSystem.Site.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BreakfastOrderSystem.Site.Models.ViewModels
{
    public class OrderDetailVm
    {

        public int Id { get; set; }
        public int OrderId { get; set; }
        public string MemberName { get; set; }
        public decimal? TotalPrice { get; set; }  // 可空的 decimal
        public int? UsedPoints { get; set; }      // 可空的 int

        public int OrderStatus { get; set; }

        
        public List<OrderItemVm> Items { get; set; }
    }

    public class OrderItemVm
    {
        public string ProductName { get; set; }
        public decimal? Price { get; set; }       // 可空的 decimal
        public int Quantity { get; set; }
        public decimal? Subtotal { get; set; }    // 可空的 decimal
        public List<OrderAddOnVm> AddOnInfo { get; set; }
        

    }

    public class OrderAddOnVm
    {
        public string AddOnName { get; set; }
        public int? AddOnQuantity { get; set; }   // 可空的 int
        public decimal? AddOnOptionPrice { get; set; }  // 可空的 decimal
        public decimal? TotalAddOnPrice { get; set; }   // 可空的 decimal
    }


}