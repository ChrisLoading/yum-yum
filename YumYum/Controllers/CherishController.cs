﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Data;
using YumYum.Models;
using YumYum.Models.DataTransferObject;
using YumYum.Models.ViewModels;
using static Azure.Core.HttpHeader;

namespace YumYum.Controllers
{
	public class CherishController : Controller
	{
		private readonly YumYumDbContext _context;

		public CherishController(YumYumDbContext context)
		{
			_context = context;
		}

		public IActionResult Introduce()
		{
			return View();
		}

		public IActionResult Manage()
		{
			// 設定 Breadcrumb
			ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
			 new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
			 new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
			 new BreadcrumbItem("管理良食", "#") // 當前的頁面
             };

			// 抓到 userId (在 UserController LogInPage 這邊有set)
			//int? userId = HttpContext.Session.GetInt32("userId");
			int userId = 3238; // 登入者Id（測試用）

			var query = from o in _context.CherishOrders
						where o.GiverUserId == userId
						orderby o.ObtainDate descending
						select new CherishManageViewModel
						{
							// [訂單類]
							GiverUserId = o.GiverUserId,
							Quantity = o.Quantity,
							EndDate = o.EndDate,
							ObtainSource = o.ObtainSource,
							ObtainDate = o.ObtainDate,
							CherishValidDate = o.CherishOrderCheck!.CherishValidDate,
							CherishPhoto = o.CherishOrderCheck.CherishPhoto,

							// [食材類]
							IngredientName = o.Ingredient.IngredientName,
							IngredAttributeName = o.IngredAttribute.IngredAttributeName,

							// [地區類]
							CityName = o.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
							RegionName = o.CherishOrderInfo.TradeRegion.RegionName,

							// [聯絡類]
							UserNickname = o.CherishOrderInfo.UserNickname,
							ContactLine = o.CherishOrderInfo.ContactLine,
							ContactPhone = o.CherishOrderInfo.ContactPhone,
							ContactOther = o.CherishOrderInfo.ContactOther,
							//TradeTimeCode
						};

			return View(query.ToList());
		}

		public IActionResult ManageAdd()
		{
			return View();
		}

		public IActionResult ManageEdit()
		{
			return View();
		}
		[HttpGet]
		public async Task<IActionResult> Match()
		{
			ViewBag.BreadcrumbsMatch = new List<BreadcrumbItem>{
			 new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
			 new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
			 new BreadcrumbItem("良食配對", "#") // 當前的頁面
             };

			var chrishOrders = from c in _context.CherishOrders
							   select new CherishMatch
							   {
								   CherishId = c.CherishId,
								   EndDate = c.EndDate,
								   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
								   IngredientName = c.Ingredient.IngredientName,
								   Quantity = c.Quantity,
								   ObtainSource = c.ObtainSource,
								   ObtainDate = c.ObtainDate,
								   UserNickname = c.GiverUser.UserNickname!,
								   CityName = c.CherishOrderInfo!.TradeCityKey,
								   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
								   ContactLine = c.CherishOrderInfo.ContactLine,
								   ContactPhone = c.CherishOrderInfo.ContactPhone,
								   ContactOther = c.CherishOrderInfo.ContactOther,
								   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
								   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
							   };

			return View(await chrishOrders.ToListAsync());
		}

		[HttpPost]
		public IActionResult Match([Bind("IngredientSelect", "CitySelect", "RegionSelect")] CherishMatchSearch search)
		{
			ViewBag.BreadcrumbsMatch = new List<BreadcrumbItem>{
			 new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
			 new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
			 new BreadcrumbItem("良食配對", "#") // 當前的頁面
             };

			var chrishSearchOrders = from c in _context.CherishOrders
									 where (search.CityKey == null || c.CherishOrderInfo!.TradeCityKey == search.CityKey) &&
										   (search.RegionId > 0 || c.CherishOrderInfo!.TradeRegionId == search.RegionId) &&
										   (search.IngredientId > 0 || c.IngredientId == search.IngredientId)
									 select new CherishMatch
									 {
										 CherishId = c.CherishId,
										 EndDate = c.EndDate,
										 IngredAttributeName = c.IngredAttribute.IngredAttributeName,
										 IngredientName = c.Ingredient.IngredientName,
										 Quantity = c.Quantity,
										 ObtainSource = c.ObtainSource,
										 ObtainDate = c.ObtainDate,
										 UserNickname = c.GiverUser.UserNickname!,
										 CityName = c.CherishOrderInfo!.TradeCityKey,
										 RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
										 ContactLine = c.CherishOrderInfo.ContactLine,
										 ContactPhone = c.CherishOrderInfo.ContactPhone,
										 ContactOther = c.CherishOrderInfo.ContactOther
									 };

			return View(chrishSearchOrders.ToListAsync());
		}








        public async Task<IActionResult> MatchHistory()
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("配對紀錄", Url.Action("MatchHistoryMine", "Cherish") ?? "#"),
             new BreadcrumbItem("我的食材", "#") // 當前的頁面
             };

            int userId = 3201;
            var chrishOrders = from c in _context.CherishOrders
                               where c.GiverUserId == userId
                               select new MatchHistory
                               {
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKey,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
                               };

            return View(await chrishOrders.ToListAsync());
        }
        public async Task<IActionResult> MatchHistoryOthers()
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("配對紀錄", Url.Action("MatchHistoryOthers", "Cherish") ?? "#"),
             new BreadcrumbItem("別人的食材", "#") // 當前的頁面
             };
            int userId = 3201;
            var chrishOrders = from c in _context.CherishOrders
                               join coa in _context.CherishOrderApplicants
                               on c.CherishId equals coa.CherishId
                               where coa.ApplicantId == userId
                               select new MatchHistory
                               {

                                   ApplicantId = coa.ApplicantId,
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKey,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
                               };

            return View(await chrishOrders.ToListAsync());
        }

        [Route("Cherish/MatchHistoryMineInfo/{cherishId}")]
        public async Task<IActionResult> MatchHistoryMineInfo(int cherishId)
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("配對紀錄", Url.Action("MatchHistoryMine", "Cherish") ?? "#"),
             new BreadcrumbItem("我的食材", "#") // 當前的頁面
             };

            int userId = 3201; // 假設目前登入的使用者 ID 是固定的
            var chrishOrders = from c in _context.CherishOrders
                               join coa in _context.CherishOrderApplicants
                               on c.CherishId equals coa.CherishId
                               where c.GiverUserId == userId && c.CherishId == cherishId
                               select new MatchHistory
                               {
                                   UserNickname = coa.UserNickname,
                                   ApplicantId = coa.ApplicantId,
                                   ApplicantContactLine = coa.ApplicantContactLine,
                                   ApplicantContactPhone = coa.ApplicantContactPhone,
                                   ApplicantContactOther = coa.ApplicantContactOther,
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   CityName = c.CherishOrderInfo!.TradeCityKey,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate
                               };

            var list = await chrishOrders.ToListAsync();

            // 偵錯: 檢查查詢結果是否有資料
            if (!list.Any())
            {
                Console.WriteLine($"No data found for cherishId {cherishId} and userId {userId}");
            }

            return View(list);
        }

        [Route("Cherish/MatchHistoryOthersInfo/{cherishId}")]
        public async Task<IActionResult> MatchHistoryOthersInfo(int cherishId)
        {

            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("配對紀錄", Url.Action("MatchHistoryOthers", "Cherish") ?? "#"),
             new BreadcrumbItem("別人的食材", "#") // 當前的頁面
             };

            //設定可面交時間

            // 先抓到該筆訂單or該預設聯絡資訊的所有面交時段
            var timeSpan = await (from d in _context.CherishTradeTimes
                                  where d.CherishId == cherishId
                                  select d).ToListAsync();

            // 進行篩選，並傳遞給ViewBag
            ViewBag.Mon = timeSpan.Where(d => d.TradeTimeCode.Contains("Mon"));
            ViewBag.Tue = timeSpan.Where(d => d.TradeTimeCode.Contains("Tue"));
            ViewBag.Wes = timeSpan.Where(d => d.TradeTimeCode.Contains("Wes"));
            ViewBag.Thr = timeSpan.Where(d => d.TradeTimeCode.Contains("Thr"));
            ViewBag.Fri = timeSpan.Where(d => d.TradeTimeCode.Contains("Fri"));
            ViewBag.Sat = timeSpan.Where(d => d.TradeTimeCode.Contains("Sat"));
            ViewBag.Sun = timeSpan.Where(d => d.TradeTimeCode.Contains("Sun"));


            var chrishOrders = from c in _context.CherishOrders
                               join coa in _context.CherishOrderApplicants
                               on c.CherishId equals coa.CherishId
                               where c.CherishId == cherishId
                               select new MatchHistory
                               {
                                   ApplicantId = coa.ApplicantId,
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKey,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
                               };

            



            return View(await chrishOrders.ToListAsync());
        }








        public IActionResult ContactInformation()
		{
			// 設定 Breadcrumb
			ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
			 new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
			 new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
			 new BreadcrumbItem("聯絡資料", "#") // 當前的頁面
             };

			return View();
		}

		[HttpGet]
		public JsonResult GetRegions(string CityKey)
		{
			var regions = _context.Regions
				.Where(r => r.CityKey == CityKey)
				.Select(r => new { r.RegionId, r.RegionName })
				.ToList();

			return Json(regions);
		}

	}
}
