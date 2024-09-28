using BreakfastOrderSystem.Site.Models.EFModels;
using BreakfastOrderSystem.Site.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BreakfastOrderSystem.Site.Controllers
{
    public class OrderlistController : Controller
    {
        private readonly AppDbContext _db;

        public OrderlistController()
        {
            _db = new AppDbContext();
        }

        // GET: Orderlist
        public ActionResult Orderlist()
        {
            var orders = _db.Orders
                 .OrderByDescending(o => o.OrderTime)  // 依照 OrderTime 進行降序排列
                            .Select(o => new OrderlistVm
                            {
                                OrderId = o.Id,
                                OrderTime = o.OrderTime,  // 使用 DateTime 類型
                                PickUpTime = o.TakeTime,  // 使用 DateTime 類型
                                TotalPrice = o.FinalTotal,
                                OrderStatus = o.OrderStatus,  // 根據你的資料表結構
                                MemberName = o.Member.Name
                            })
                            .ToList();

            return View(orders);  // 傳遞資料到 View
        }

        [HttpGet]
        public ActionResult GetOrderDetails(int orderId)
        {
            var orderDetails = _db.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new OrderDetailVm
                {
                    OrderId = o.Id, // 訂單編號
                    MemberName = o.Member.Name, // 會員名稱（從 Member 表聯結）
                    TotalPrice = o.FinalTotal, // 訂單金額
                    UsedPoints = o.PointsUsed, // 點數折抵
                    MemberID = o.Member.Id,  // 返回MemberID
                    IsBlacklisted = o.Member.BlackList,
                    Items = o.OrderDetails.Select(od => new OrderItemVm
                    {
                        ProductName = od.Product.Name, // 商品名稱
                        Price = od.ProductPrice, // 單價
                        Quantity = od.ProductQuantity, // 數量
                                                       // 這裡將小計計算為產品小計加上加選項的總價格
                        Subtotal = od.SubTotal + (od.OrderAddOnDetails.Any()
            ? od.OrderAddOnDetails.Sum(a => (decimal?)(a.AddOnQuantity * a.AddOnOptionPrice)) ?? 0
            : 0),

                        AddOnInfo = od.OrderAddOnDetails.Select(a => new OrderAddOnVm
                        {
                            AddOnName = a.ProductAddOnDetail.AddOnOptionName, // 加選資訊名稱
                            AddOnQuantity = a.AddOnQuantity, // 加選數量
                            AddOnOptionPrice = a.AddOnOptionPrice, // 加選選項價格
                            TotalAddOnPrice = a.AddOnOptionPrice * a.AddOnQuantity // 加選小計
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefault();

            if (orderDetails == null)
            {
                return Json(new { success = false, message = "訂單未找到" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = orderDetails }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ToggleBlacklist(int memberId, bool isBlacklisted)
        {
            try
            {
                // 使用您的DB上下文來尋找會員記錄
                var member = _db.Members.FirstOrDefault(m => m.Id == memberId);
                if (member == null)
                {
                    return Json(new { success = false, message = "會員不存在" });
                }

                // 更新黑名單狀態
                member.BlackList = isBlacklisted;
                _db.SaveChanges();  // 保存更改

                return Json(new { success = true, message = "黑名單狀態已更新", isBlacklisted = member.BlackList });    
            }
            catch (Exception ex)
            {
                // 捕捉任何異常
                return Json(new { success = false, message = "更新失敗: " + ex.Message });
            }
        }

    }
}
