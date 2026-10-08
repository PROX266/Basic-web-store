using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly WebstoreContext _context;
        private readonly CatalogCache _cache;

        public CategoriesController(WebstoreContext context, CatalogCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View(_context.Categories.OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("NameCategories")] Category item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                _context.SaveChanges();
                _cache.ResetCategories();
                TempData["Message"] = "Запись добавлена";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = _context.Categories.Find(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,NameCategories")] Category item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(item);
                _context.SaveChanges();
                _cache.ResetCategories();
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
                _cache.ResetCategories();
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

        private Category? Load(int id)
        {
            return _context.Categories.FirstOrDefault(x => x.Id == id);
        }
    }
}
