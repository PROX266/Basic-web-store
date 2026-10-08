using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly WebstoreContext _context;

        public RolesController(WebstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Roles.OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("RoleName")] Role item)
        {
            if (_context.Roles.Any(r => r.RoleName == item.RoleName && r.Id != item.Id))
                ModelState.AddModelError("RoleName", "Такая роль уже существует");

            if (ModelState.IsValid)
            {
                _context.Add(item);
                _context.SaveChanges();
                TempData["Message"] = "Запись добавлена";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = _context.Roles.Find(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,RoleName")] Role item)
        {
            if (id != item.Id) return NotFound();

            if (_context.Roles.Any(r => r.RoleName == item.RoleName && r.Id != item.Id))
                ModelState.AddModelError("RoleName", "Такая роль уже существует");

            if (ModelState.IsValid)
            {
                _context.Update(item);
                _context.SaveChanges();
                TempData["Message"] = "Запись изменена";
                return RedirectToAction("Index");
            }
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

        private Role? Load(int id)
        {
            return _context.Roles.FirstOrDefault(x => x.Id == id);
        }
    }
}
