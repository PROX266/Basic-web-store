using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Home/StatusCode")]
        public IActionResult StatusCodePage(int code)
        {
            Response.StatusCode = code;
            ViewBag.Code = code;
            ViewBag.Text = code switch
            {
                403 => "Доступ запрещён",
                404 => "Страница не найдена",
                _ => "Что-то пошло не так"
            };
            return View("StatusCode");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (feature != null)
                _logger.LogError(feature.Error, "Необработанная ошибка");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
