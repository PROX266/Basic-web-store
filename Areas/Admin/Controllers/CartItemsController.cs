using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CartItemsController : Controller
    {
        private readonly WebstoreContext _context;

        public CartItemsController(WebstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.CartItems.Include(x => x.Cart).ThenInclude(c => c.User).Include(x => x.Product).OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            FillLists();
            return View(new CartItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("CartId,ProductId,Quantity")] CartItem item)
        {
            ModelState.Remove("Cart");
            ModelState.Remove("Product");
            if (item.Quantity <= 0)
                ModelState.AddModelError("Quantity", "Количество должно быть больше 0");

            if (ModelState.IsValid)
            {
                _context.Add(item);
                _context.SaveChanges();
                TempData["Message"] = "Запись добавлена";
                return RedirectToAction("Index");
            }
            FillLists();
            return View(item);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = _context.CartItems.Find(id);
            if (item == null) return NotFound();

            FillLists();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,CartId,ProductId,Quantity")] CartItem item)
        {
            if (id != item.Id) return NotFound();

            ModelState.Remove("Cart");
            ModelState.Remove("Product");
            if (item.Quantity <= 0)
                ModelState.AddModelError("Quantity", "Количество должно быть больше 0");

            if (ModelState.IsValid)
            {
                _context.Update(item);
                _context.SaveChanges();
                TempData["Message"] = "Запись изменена";
                return RedirectToAction("Index");
            }
            FillLists();
            return View(item);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = Load(id.Value);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = Load(id);
            if (item == null) return NotFound();

            try
            {
                _context.Remove(item);
                _context.SaveChanges();
                TempData["Message"] = "Запись удалена";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                ViewBag.Error = "Нельзя удалить запись: на неё ссылаются другие таблицы. Сначала удалите связанные записи.";
                return View("Delete", Load(id));
            }
        }

        private CartItem? Load(int id)
        {
            return _context.CartItems.Include(x => x.Cart).ThenInclude(c => c.User).Include(x => x.Product).FirstOrDefault(x => x.Id == id);
        }

        private void FillLists()
        {
            var carts = _context.Carts.Include(c => c.User).ToList();
            ViewBag.Carts = new SelectList(carts.Select(c => new { c.Id, Name = "Корзина №" + c.Id + " (" + c.User.UserLogin + ")" }), "Id", "Name");
            ViewBag.Products = new SelectList(_context.Products.ToList(), "Id", "ProductName");
        }
    }
}
