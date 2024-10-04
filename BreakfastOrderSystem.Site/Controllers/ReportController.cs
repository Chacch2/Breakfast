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

        // 通過月份篩選數據，返回 JSON 格式
        [HttpGet]
        public ActionResult GetReportData2(string startMonth, string endMonth)
        {
            DateTime today = DateTime.Now;

            DateTime startDate, endDate;

            // 如果沒有提供月份範圍，預設為最近四個月
            if (string.IsNullOrEmpty(startMonth) || string.IsNullOrEmpty(endMonth))
            {
                startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-3);
                endDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }
            else
            {
                // 根據用戶選擇的月份範圍解析 startDate 和 endDate
                startDate = DateTime.Parse(startMonth + "-01");
                endDate = DateTime.Parse(endMonth + "-01").AddMonths(1).AddTicks(-1);  // 包含選定月份的整個月份
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

            var reportVm = new ReportVm
            {
                SalesData = salesData
            };

            return Json(reportVm, JsonRequestBehavior.AllowGet);
        }

        // 顯示商品銷售統計頁面
        [HttpGet]
        public ActionResult Index3()
        {
            return View();  // 回傳商品銷售統計視圖
        }

        // 商品銷售統計資料
        [HttpGet]
        public ActionResult GetReportData3(string startDate, string endDate, int page = 1)
        {
            const int pageSize = 5; // 每頁顯示 5 個商品

            // 處理日期範圍
            DateTime? startDateTime = null;
            DateTime? endDateTime = null;
            if (!string.IsNullOrEmpty(startDate))
                startDateTime = DateTime.Parse(startDate);
            if (!string.IsNullOrEmpty(endDate))
                endDateTime = DateTime.Parse(endDate);

            // 查詢訂單詳情，按產品名稱分組，計算數量總和
            var query = _db.OrderDetails
                .Where(od => (!startDateTime.HasValue || od.Order.OrderTime >= startDateTime) &&
                             (!endDateTime.HasValue || od.Order.OrderTime <= endDateTime))
                .GroupBy(od => od.Product.Name)
                .Select(g => new ProductSalesVm
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(od => od.ProductQuantity)
                });

            // 按商品數量排序，數量相同時按產品名稱筆畫排序
            query = query.OrderByDescending(p => p.Quantity)
                         .ThenBy(p => p.ProductName.Length);

            // 計算總頁數
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // 分頁
            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 構建分頁和結果模型
            var result = new
            {
                Products = products,
                Pagination = new
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = totalItems
                }
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
