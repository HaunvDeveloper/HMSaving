using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly QlchiTieuContext _context;
        public AccountController(QlchiTieuContext context)
        {
            _context = context;
        }
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            TempData.Clear();
            return View();
        }

        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(User model, string ReturnUrl)
        {
            TempData.Clear();
            var user = _context.Users
                .FirstOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user == null)
            {
                return Json(new { status = false, message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, "admin"),
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);


                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(3)
                };

                await HttpContext.SignInAsync(principal, authProperties);

                return Json(new { status = true, message = "Đăng nhập thành công!", redirectUrl = string.IsNullOrEmpty(ReturnUrl) ? Url.Action("Index", "Jar") : ReturnUrl });
            }
        }

        public async Task<IActionResult> Logout()
        {
            TempData.Clear();
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
