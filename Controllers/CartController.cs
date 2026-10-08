using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const int MaxQuantity = 99;

        private readonly WebstoreContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(WebstoreContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Идентификатор текущего пользователя берём только из Claims, а не из запроса
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private Cart GetOrCreateCart(int userId)
        {
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return cart;
        }

        public IActionResult Index()
        {
            return View(GetOrCreateCart(CurrentUserId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            quantity = Math.Clamp(quantity, 1, MaxQuantity);

            if (!_context.Products.Any(p => p.Id == productId))
                return NotFound();

            var cart = GetOrCreateCart(CurrentUserId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }
            else
            {
                cartItem.Quantity = Math.Min(cartItem.Quantity + quantity, MaxQuantity);
            }

            _context.SaveChanges();
            TempData["Message"] = "Товар добавлен в корзину";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            // позиция ищется только среди позиций корзины текущего пользователя
            var item = _context.CartItems
                .FirstOrDefault(ci => ci.Id == id && ci.Cart.UserId == CurrentUserId);
            if (item == null) return NotFound();

            if (quantity <= 0)
                _context.CartItems.Remove(item);
            else
                item.Quantity = Math.Min(quantity, MaxQuantity);

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            var item = _context.CartItems
                .FirstOrDefault(ci => ci.Id == id && ci.Cart.UserId == CurrentUserId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            int userId = CurrentUserId;

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Корзина пуста — оформлять нечего";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            foreach (var ci in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price   // цена фиксируется на момент заказа
                });
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems.ToList());

            // один SaveChanges = одна транзакция: либо заказ создан и корзина очищена, либо ничего
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Не удалось оформить заказ пользователя {UserId}", userId);
                _context.ChangeTracker.Clear();
                TempData["Error"] = "Не удалось оформить заказ. Попробуйте ещё раз";
                return RedirectToAction("Index");
            }

            TempData["Message"] = $"Заказ №{order.Id} оформлен";
            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }
    }
}
