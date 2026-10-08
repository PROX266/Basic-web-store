using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly WebstoreContext _context;

        public OrdersController(WebstoreContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public IActionResult Index()
        {
            int userId = CurrentUserId;

            var orders = _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            int userId = CurrentUserId;

            // чужой заказ найти нельзя: фильтр по UserId из Claims
            var order = _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
