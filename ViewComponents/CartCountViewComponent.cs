using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly WebstoreContext _context;

        public CartCountViewComponent(WebstoreContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            int count = 0;
            var idValue = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(idValue, out int userId))
            {
                count = _context.CartItems
                    .Where(ci => ci.Cart.UserId == userId)
                    .Sum(ci => (int?)ci.Quantity) ?? 0;
            }

            return View(count);
        }
    }
}
