﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using YumYum.Models;
using YumYum.Models.ViewModels;

namespace YumYum.Controllers
{
    public class UserController : Controller
    {
        //引用資料庫
        private readonly YumYumDbContext _context;

        public UserController(YumYumDbContext context)
        {
            _context = context;
        }





		//健誠
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			int? userId = HttpContext.Session.GetInt32("userId");
			//int? userId = 3207;

			var userQuery = from user in _context.UserBios
							join userNickname in _context.UserSecretInfos
							on user.UserId equals userNickname.UserId
							where user.UserId == userId
							select new UserQueryViewModel
							{
								UserId = user.UserId,
								UserIntro = user.UserIntro,
								HeadShot = user.HeadShot,
								Igaccount = user.Igaccount,
								Fbnickname = user.Fbnickname,
								YoutuNickname = user.YoutuNickname,
								WebNickName = user.WebNickName,
								YoutuLink = user.YoutuLink,
								Fblink = user.Fblink,
								WebLink = user.WebLink,
								UserNickname = userNickname.UserNickname
							};

			var recipeQuery = from user in userQuery
							  join recipe in _context.RecipeBriefs
							  on user.UserId equals recipe.CreatorId
							  join recipeInfo in _context.RecipeRecords
							  on recipe.RecipeId equals recipeInfo.RecipeId
							  join recipeImage in _context.RecipeRecordFields
							  on recipe.RecipeId equals recipeImage.RecipeId
							  where recipeInfo.RecipeStatusCode == 1 || recipeInfo.RecipeStatusCode == 4
							  where recipeImage.RecipeField == 0
							  select new RecipeQueryViewModel
							  {
								  RecipeId = recipe.RecipeId,
								  RecipeName = recipe.RecipeName,
								  RecipeStatusCode = recipeInfo.RecipeStatusCode,
								  FieldShot = recipeImage.FieldShot,
							  };

			var recipeDetailQuery = from recipe in recipeQuery
									join recipeIngredient in _context.RecipeIngredients on recipe.RecipeId equals recipeIngredient.RecipeId
									join recipeIngredientName in _context.Ingredients on recipeIngredient.IngredientId equals recipeIngredientName.IngredientId
									select new RecipeDetailQuery
									{
										RecipeId = recipeIngredient.RecipeId,
										IngredientId = recipeIngredient.IngredientId,
										IngredientName = recipeIngredientName.IngredientName
									};
			var AllList = new RecipeAllUser()
			{
				userQueryViewModel = userQuery.ToList(),
				recipeQueryViewModel = recipeQuery.ToList(),
				recipeDetailQuery = recipeDetailQuery.ToList(),
			};

			HttpContext.Session.SetInt32("userId", (int)userId);
			return View(AllList);


		}


		[HttpGet]
		public async Task<IActionResult> EditInfo()
		{
			int? userId = HttpContext.Session.GetInt32("userId");
			//int? userId = 3207;//for test
			UserSecretInfo? userSecretInfo = await _context.UserSecretInfos.Where(p => p.UserId == userId).FirstOrDefaultAsync();
			UserBio? userBio = await _context.UserBios.Where(p => p.UserId == userId).FirstOrDefaultAsync();
			if (userSecretInfo == null || userBio == null)
			{
				return NotFound();
			}
			var viewModel = new UserViewModel
			{
				UserId = userBio.UserId,
				HeadShot = userBio.HeadShot,
				UserIntro = userBio.UserIntro,
				Igaccount = userBio.Igaccount,
				Fbnickname = userBio.Fbnickname,
				YoutuNickname = userBio.YoutuNickname,
				WebNickName = userBio.WebNickName,
				YoutuLink = userBio.YoutuLink,
				Fblink = userBio.Fblink,
				WebLink = userBio.WebLink,
				UserNickname = userSecretInfo.UserNickname
			};
			HttpContext.Session.SetInt32("userId", (int)userId);
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditInfo(UserViewModel viewModel)
		{
			int? userId = HttpContext.Session.GetInt32("userId");

			//int? userId = 3207;//for test					

			if (ModelState.IsValid)
			{
				UserSecretInfo? userSecretInfo = await _context.UserSecretInfos.Where(p => p.UserId == userId).FirstOrDefaultAsync();
				UserBio? userBio = await _context.UserBios.Where(p => p.UserId == userId).FirstOrDefaultAsync();

				if (userSecretInfo == null || userBio == null)
				{
					return NotFound();
				}

				userBio.UserIntro = viewModel.UserIntro;
				userBio.HeadShot = viewModel.HeadShot;
				userBio.Igaccount = viewModel.Igaccount;
				userBio.Fbnickname = viewModel.Fbnickname;
				userBio.YoutuNickname = viewModel.YoutuNickname;
				userBio.WebNickName = viewModel.WebNickName;
				userBio.YoutuLink = viewModel.YoutuLink;
				userBio.Fblink = viewModel.Fblink;
				userBio.WebLink = viewModel.WebLink;
				userSecretInfo.UserNickname = viewModel.UserNickname;

				_context.Update(userBio);
				_context.Update(userSecretInfo);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			HttpContext.Session.SetInt32("userId", (int)userId);
			return View(viewModel);

		}
		private bool UserBioExists(int id)
		{
			return _context.UserBios.Any(e => e.UserId == id);
		}




		//芳慈
		public IActionResult MyRecipeEdit()
        {
            // 設定Breadcrumb 顯示頁面資訊
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
             {
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("會員專區", Url.Action("Index", "User") ?? "#"),
             new BreadcrumbItem("我的食譜", "#") // 目前的頁面
             };


            //sql db test--成功連上
            string? RecipeName = (from xa in _context.RecipeBriefs
                                  where xa.RecipeId == 1399
                                  select xa.RecipeName).SingleOrDefault();
            ViewBag.recipeName = RecipeName;

            return View();

        }

        public IActionResult MyRecipeCollect()
        {

            // 設定Breadcrumb 顯示頁面資訊
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
            new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
            new BreadcrumbItem("會員專區", Url.Action("Index", "User") ?? "#"),
            new BreadcrumbItem("我的食譜", Url.Action("MyRecipeEdit", "User") ?? "#"),
            new BreadcrumbItem("收藏食譜", "#") // 目前的頁面
            };

            //return View();


            //sql db test2
            var data = from xa in _context.RecipeBriefs
                       where xa.RecipeId > 1399 && xa.RecipeId <= 1404

                       select new RecipeBrief
                       {
                           RecipeName = xa.RecipeName,
                           //RecipeShot = xa.RecipeShot
                       };

            return View(data.ToList());
        }


        //毅祥
        public async Task<IActionResult> LogInPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogInPage([FromBody] UserSecretInfo user)
        {
            UserSecretInfo users = await _context.UserSecretInfos.Where(p => p.Email == user.Email).FirstOrDefaultAsync();

            if (users != null)
            {
                HttpContext.Session.SetInt32("userId", users.UserId);
                return Json(new { redirectUrl = Url.Action("Index", "Recipe") });
            }
            else
            {
                return Json(new { errorMessage = "帳號未註冊" });
            }
        }

        public async Task<IActionResult> RegisterPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPage([FromBody] UserSecretInfo user)
        {
            UserSecretInfo users = await _context.UserSecretInfos.Where(p => p.Email == user.Email).FirstOrDefaultAsync();

            if (users != null)
            {
                return Json(new { registerdMessage = "信箱已有人使用，請更換信箱" });
            }
            else
            {
                //驗證碼
                var random = new Random();
                string varifycode = "";
                for (int i = 0; i < 6; i++)
                {
                    varifycode += random.Next(0, 10);
                }
                //將資料傳遞給驗證頁面
                //將驗證碼資料新增進user裡
                user.EmailValidCode = varifycode;
                HttpContext.Session.SetString("registerInformation", System.Text.Json.JsonSerializer.Serialize(user));
                //寄送驗證信
                try
                {
                    //SMTP設置
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("yumyumService@gmail.com", "keiqmcldmxuiqmon"),
                        EnableSsl = true,
                    };
                    // 設定郵件內容
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("yumyumService@gmail.com"),
                        Subject = "YumYum註冊會員驗證信",
                        Body = $"感謝您註冊yumyum會員，以下是您的驗證碼「{varifycode}」",
                        IsBodyHtml = true, // 設定為 HTML 格式
                    };
                    mailMessage.To.Add(user.Email);
                    //發送郵件
                    await smtpClient.SendMailAsync(mailMessage);
                    return Json(new { action = Url.Action("RegisterVerifyPage", "user"), mailsuccess = "寄送信件成功" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"郵件發送失敗: {ex.Message}", action = Url.Action("RegisterVerifyPage", "user") });
                }

            }
        }
        public async Task<IActionResult> RegisterVerifyPage()
        {
            //註冊者資訊
            string registerInfor = HttpContext.Session.GetString("registerInformation")!;
            if (!string.IsNullOrEmpty(registerInfor))
            {
                var JsonData = JsonConvert.DeserializeObject<UserSecretInfo>(registerInfor);
                return View("RegisterVerifyPage", JsonData);
            }
            else
            {
                return View();
            }

        }
        [HttpPost]
        public async Task<IActionResult> RegisterVerifyPage([FromBody] UserSecretInfo user)
        {
            await _context.UserSecretInfos.AddAsync(user);
            await _context.SaveChangesAsync();
            return Json(new { action = Url.Action("Index", "Recipe"), successmessage = "成功新增會員資料" });
        }


        [HttpPost]
        public async Task<IActionResult> SendVerifyAgain([FromBody] string Email)
        {
            //驗證碼
            var random = new Random();
            string varifycode = "";
            for (int i = 0; i < 6; i++)
            {
                varifycode += random.Next(0, 10);
            }
            string registerInfor = HttpContext.Session.GetString("registerInformation")!;
            if (!string.IsNullOrEmpty(registerInfor))
            {
                var registerInfroChange = System.Text.Json.JsonSerializer.Deserialize<UserSecretInfo>(registerInfor);
                registerInfroChange.EmailValidCode = varifycode;
                HttpContext.Session.SetString("registerInformation", System.Text.Json.JsonSerializer.Serialize(registerInfroChange));
            }
            //寄送驗證信
            try
            {
                //SMTP設置
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("yumyumService@gmail.com", "keiqmcldmxuiqmon"),
                    EnableSsl = true,
                };
                // 設定郵件內容
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("yumyumService@gmail.com"),
                    Subject = "YumYum註冊會員驗證信",
                    Body = $"感謝您註冊yumyum會員，以下是您的驗證碼「{varifycode}」",
                    IsBodyHtml = true, // 設定為 HTML 格式
                };
                mailMessage.To.Add(Email);
                //發送郵件
                await smtpClient.SendMailAsync(mailMessage);
                return Json(new { sucessmessage = "重新寄送成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"郵件發送失敗: {ex.Message}", action = Url.Action("RegisterVerifyPage", "user") });
            }
        }



        public async Task<IActionResult> ChangePasswordPage()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            UserSecretInfo user = await _context.UserSecretInfos.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userId != null)
            {
                return View(user);
            }
            else
            {
                return View();
            }

        }
        [HttpPut]
        public async Task<IActionResult> ChangePasswordPage([FromBody] UserSecretInfo user)
        {
            UserSecretInfo changePasswordUser = await _context.UserSecretInfos.FirstOrDefaultAsync(p => p.Email == user.Email);
            changePasswordUser!.Password = user.Password;
            var saveResult = await _context.SaveChangesAsync();
            if (saveResult != 0)
            {
                return Json(new { message = "變更密碼成功" });
            }
            else { return Json(new { message = "資料存取發生錯誤" }); }
        }
    }
}
