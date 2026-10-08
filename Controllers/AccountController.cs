using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockTime = TimeSpan.FromMinutes(5);

        private readonly WebstoreContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AccountController> _logger;

        public AccountController(WebstoreContext context, IMemoryCache cache, ILogger<AccountController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // ключ: логин + IP, чтобы нельзя было подбирать пароль
        private string AttemptsKey(string login) =>
            $"login_fail:{login.ToLowerInvariant()}:{HttpContext.Connection.RemoteIpAddress}";

        private void RegisterFailure(string login)
        {
            var key = AttemptsKey(login);
            int count = _cache.TryGetValue(key, out int c) ? c : 0;
            _cache.Set(key, count + 1, LockTime);
            _logger.LogWarning("Неудачная попытка входа для логина {Login}", login);
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (_context.Users.Any(u => u.UserLogin == model.Login))
                ModelState.AddModelError("Login", "Такой логин уже занят");

            if (!ModelState.IsValid)
                return View(model);

            var role = _context.Roles.FirstOrDefault(r => r.RoleName == "User");
            if (role == null)
            {
                role = new Role { RoleName = "User" };
                _context.Roles.Add(role);
                _context.SaveChanges();
            }

            var user = new User
            {
                UserLogin = model.Login,
                UserName = model.UserName,
                UserPassword = PasswordHelper.Hash(model.Password),
                RoleId = role.Id,
                IsActive = true
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            user.Role = role;
            await SignIn(user);
            return RedirectByRole(user);
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            if (_cache.TryGetValue(AttemptsKey(model.Login), out int failed) && failed >= MaxFailedAttempts)
            {
                ModelState.AddModelError("", "Слишком много неудачных попыток. Повторите вход через 5 минут");
                return View(model);
            }

            var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserLogin == model.Login);
            if (user == null)
            {
                RegisterFailure(model.Login);
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Учётная запись отключена");
                return View(model);
            }

            if (!PasswordHelper.Verify(model.Password, user.UserPassword))
            {
                RegisterFailure(model.Login);
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            if (user.UserPassword.Length == 32)
            {
                user.UserPassword = PasswordHelper.Hash(model.Password);
                _context.SaveChanges();
            }

            _cache.Remove(AttemptsKey(model.Login));
            await SignIn(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectByRole(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }


        private async Task SignIn(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Login", user.UserLogin),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        }

        private IActionResult RedirectByRole(User user)
        {
            if (user.Role.RoleName == "Admin")
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            return RedirectToAction("Index", "Products");
        }
    }
}
