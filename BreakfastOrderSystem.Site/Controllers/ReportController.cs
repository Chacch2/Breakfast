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
        public ActionResult GetReportData()
        {
            var reportData = new
            {
                // 計算總營業額
                TotalSales = _db.Orders.Sum(o => o.Total),

                // 計算總訂單數
                TotalOrders = _db.Orders.Count(),

                // 計算不同的會員數，distinct 是用來確保不重複的會員ID
                TotalMembers = _db.Orders.Select(o => o.MemberID).Distinct().Count()
            };

            return Json(reportData, JsonRequestBehavior.AllowGet); // 回傳 JSON 給前端
        }
    }
}
