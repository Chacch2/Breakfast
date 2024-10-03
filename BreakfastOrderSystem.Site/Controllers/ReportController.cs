using BreakfastOrderSystem.Site.Models.EFModels;
using BreakfastOrderSystem.Site.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BreakfastOrderSystem.Site.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;

        public ReportController()
        {
            _db = new AppDbContext();
        }

        // 這個 Action 負責顯示報告頁面
        [HttpGet]
        public ActionResult Index()
        {
            return View(); // 這裡會尋找 Views/Report/Index.cshtml 作為視圖頁面
        }

        // 用來提供營業數據的 API
        [HttpGet]
        public ActionResult GetReportData(DateTime? startDate, DateTime? endDate)
        {
            // 如果沒有提供日期範圍，使用預設的範圍
            if (startDate == null)
            {
                startDate = DateTime.MinValue; // 沒有開始日期時，使用最早的日期
            }
            if (endDate == null)
            {
                endDate = DateTime.MaxValue; // 沒有結束日期時，使用最晚的日期
            }
            else
            {
                // 將 endDate 設置為當天的最後一秒，確保包含當天的所有訂單
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }


            // 查詢在指定日期範圍內的訂單
            var filteredOrders = _db.Orders
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate);

            var reportData = new
            {
                // 計算篩選後的總營業額
                TotalSales = filteredOrders.Sum(o => (decimal?)o.Total) ?? 0,

                // 計算篩選後的總訂單數
                TotalOrders = filteredOrders.Count(),

                // 計算不同的會員數，distinct 確保不重複的會員ID
                TotalMembers = filteredOrders.Select(o => o.MemberID).Distinct().Count()
            };

            // 回傳 JSON 資料
            return Json(reportData, JsonRequestBehavior.AllowGet);
        }
        // 顯示報表頁面
        [HttpGet]
        public ActionResult Index2()
        {
            return View();  // 回傳網頁視圖
        }

        // 通過日期篩選數據，返回 JSON 格式
        [HttpGet]
        public ActionResult GetReportData2(DateTime? startDate, DateTime? endDate)
        {
            DateTime today = DateTime.Now;

            // 如果沒有提供日期範圍，預設為最近四個月
            if (startDate == null || endDate == null)
            {
                // 最近四個月
                startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-3);
                endDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }
            else
            {
                // 查詢自定義區間，確保包括選定的最後一日
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            // 計算最近四個月範圍的月份
            var months = Enumerable.Range(0, 4)
                .Select(i => today.AddMonths(-i))
                .OrderBy(m => m)
                .ToList();

            // 營業數據
            var salesData = _db.Orders
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate)
                .GroupBy(o => new { o.OrderTime.Year, o.OrderTime.Month })
                .Select(g => new SalesDataVm
                {
                    Month = g.Key.Month,
                    TotalSales = g.Sum(o => (decimal?)o.Total) ?? 0,
                    TotalMembers = g.Select(o => o.MemberID).Distinct().Count()
                })
                .ToList();

            // 填充沒有資料的月份，確保最近四個月全部顯示
            foreach (var month in months)
            {
                if (!salesData.Any(d => d.Month == month.Month))
                {
                    salesData.Add(new SalesDataVm
                    {
                        Month = month.Month,
                        TotalSales = 0,
                        TotalMembers = 0
                    });
                }
            }

            // 根據月份排序
            salesData = salesData.OrderBy(d => d.Month).ToList();

            // 熱銷商品
            var hotItems = _db.OrderDetails
                .Where(od => od.Order.OrderTime >= startDate && od.Order.OrderTime <= endDate)
                .GroupBy(od => od.Product.Name)
                .Select(g => new HotItemVm
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(od => od.ProductQuantity)
                })
                .OrderByDescending(g => g.Quantity)
                .Take(4)
                .ToList();

            var reportVm = new ReportVm
            {
                SalesData = salesData,
                HotItems = hotItems
            };

            return Json(reportVm, JsonRequestBehavior.AllowGet);
        }
    }
}
