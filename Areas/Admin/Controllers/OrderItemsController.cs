using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderItemsController : Controller
    {
        private readonly WebstoreContext _context;

        public OrderItemsController(WebstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.OrderItems.Include(x => x.Product).OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            FillLists();
            return View(new OrderItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("OrderId,ProductId,Quantity,Price")] OrderItem item)
        {
            ModelState.Remove("Order");
            ModelState.Remove("Product");
            if (item.Quantity <= 0)
                ModelState.AddModelError("Quantity", "Количество должно быть больше 0");
            if (item.Price < 0)
                ModelState.AddModelError("Price", "Цена не может быть отрицательной");

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

            var item = _context.OrderItems.Find(id);
            if (item == null) return NotFound();

            FillLists();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,OrderId,ProductId,Quantity,Price")] OrderItem item)
        {
            if (id != item.Id) return NotFound();

            ModelState.Remove("Order");
            ModelState.Remove("Product");
            if (item.Quantity <= 0)
                ModelState.AddModelError("Quantity", "Количество должно быть больше 0");
            if (item.Price < 0)
                ModelState.AddModelError("Price", "Цена не может быть отрицательной");

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

        private OrderItem? Load(int id)
        {
            return _context.OrderItems.Include(x => x.Product).FirstOrDefault(x => x.Id == id);
        }

        private void FillLists()
        {
            var orders = _context.Orders.Include(o => o.User).ToList();
            ViewBag.Orders = new SelectList(orders.Select(o => new { o.Id, Name = "Заказ №" + o.Id + " (" + o.User.UserLogin + ")" }), "Id", "Name");
            ViewBag.Products = new SelectList(_context.Products.ToList(), "Id", "ProductName");
        }
    }
}
