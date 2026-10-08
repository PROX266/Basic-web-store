using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly WebstoreContext _context;

        public OrdersController(WebstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Orders.Include(x => x.User).Include(x => x.OrderItems).OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            FillLists();
            return View(new Order { CreatedAt = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("UserId,CreatedAt")] Order item)
        {
            ModelState.Remove("User");
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

            var item = _context.Orders.Find(id);
            if (item == null) return NotFound();

            FillLists();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,UserId,CreatedAt")] Order item)
        {
            if (id != item.Id) return NotFound();

            ModelState.Remove("User");
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

        private Order? Load(int id)
        {
            return _context.Orders.Include(x => x.User).Include(x => x.OrderItems).FirstOrDefault(x => x.Id == id);
        }

        private void FillLists()
        {
            ViewBag.Users = new SelectList(_context.Users.ToList(), "Id", "UserLogin");
        }
    }
}
