using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly WebstoreContext _context;

        public HomeController(WebstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Roles = _context.Roles.Count();
            ViewBag.Users = _context.Users.Count();
            ViewBag.Categories = _context.Categories.Count();
            ViewBag.Products = _context.Products.Count();
            ViewBag.Carts = _context.Carts.Count();
            ViewBag.CartItems = _context.CartItems.Count();
            ViewBag.Orders = _context.Orders.Count();
            ViewBag.OrderItems = _context.OrderItems.Count();
            return View();
        }
    }
}
