using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BreakfastOrderSystem.Site.Models.ViewModels
{
    public class OrderlistVm
    {
        public int OrderId { get; set; }            // 訂單編號
        public DateTime OrderTime { get; set; }        // 下單時間
        public DateTime PickUpTime { get; set; }       // 取餐時間
        public decimal TotalPrice { get; set; }      // 總金額
        public int OrderStatus { get; set; }      // 訂單狀態
        public string MemberName { get; set; }       // 會員名稱
    }
}   