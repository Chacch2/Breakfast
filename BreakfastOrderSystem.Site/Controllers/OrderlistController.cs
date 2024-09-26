using BreakfastOrderSystem.Site.Models.EFModels;
using BreakfastOrderSystem.Site.Models.ViewModels;
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

    }
}
