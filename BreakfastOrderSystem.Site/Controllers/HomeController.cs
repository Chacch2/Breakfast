using BreakfastOrderSystem.Site.Models.EFModels;
using BreakfastOrderSystem.Site.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BreakfastOrderSystem.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db; // 使用 _db 來表示資料庫上下文

       

        public HomeController() // 將 context 改為 db
        {
            _db = new AppDbContext(); 
        }

        public ActionResult Index()
        {
            // 查詢訂單及相關資料
            var orders = (from o in _db.Orders
                          join m in _db.Members on o.MemberID equals m.Id
                          where o.OrderStatus == 1
                          select new OrderDetailVm
                          {
                              Id = o.Id,
                              OrderId = o.TakeOrderNumber,
                              MemberName = m.Name,
                              TotalPrice = o.FinalTotal,
                              OrderStatus = o.OrderStatus, // 新增此屬性，抓取訂單狀態
                              Items = (from od in _db.OrderDetails
                                       where od.OrderID == o.Id
                                       select new OrderItemVm
                                       {
                                           ProductName = od.ProductName,
                                           Price = od.ProductPrice,
                                           Quantity = od.ProductQuantity,
                                           Subtotal = od.ProductPrice * od.ProductQuantity, // 小計 = 單價 * 數量

                                           // 加選資訊：根據 OrderAddOnDetails 的 ProductAddOnDetailsID 關聯 ProductAddOnDetails
                                           AddOnInfo = (from oad in _db.OrderAddOnDetails
                                                        join pao in _db.ProductAddOnDetails on oad.ProductAddOnDetailsID equals pao.Id
                                                        where oad.OrderDetailID == od.Id
                                                        select new OrderAddOnVm
                                                        {
                                                            AddOnName = pao.AddOnOptionName, // 抓取加選名稱
                                                            AddOnQuantity = oad.AddOnQuantity // 加選數量
                                                        }).ToList()  // 將加選資訊轉為列表
                                       }).ToList()  // 將商品資訊轉為列表
                          }).ToList();  // 將訂單資料轉為列表

            // 將資料傳遞給視圖
            return View(orders);
        }


        [HttpPost]
        public ActionResult UpdateOrderStatus(OrderDetailVm model)
        {
            try
            {
                // 根據 model.OrderId 查找訂單
                var order = _db.Orders.FirstOrDefault(o => o.TakeOrderNumber == model.OrderId);

                if (order != null)
                {
                    // 更新訂單狀態
                    order.OrderStatus = model.OrderStatus;

                    // 保存更改
                    _db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "訂單不存在" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}