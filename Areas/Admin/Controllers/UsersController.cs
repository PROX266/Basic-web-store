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
    public class UsersController : Controller
    {
        private readonly WebstoreContext _context;

        public UsersController(WebstoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Users.Include(x => x.Role).OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            FillLists();
            return View(new User { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("UserLogin,UserName,RoleId,IsActive")] User item, string? password)
        {
            ModelState.Remove("Role");
            ModelState.Remove("UserPassword");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("UserPassword", "Введите пароль");

            if (_context.Users.Any(u => u.UserLogin == item.UserLogin))
                ModelState.AddModelError("UserLogin", "Такой логин уже существует");

            if (ModelState.IsValid)
            {
                item.UserPassword = PasswordHelper.Hash(password!);
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

            var item = _context.Users.Find(id);
            if (item == null) return NotFound();

            FillLists();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,UserLogin,UserName,RoleId,IsActive")] User item, string? password)
        {
            if (id != item.Id) return NotFound();

            ModelState.Remove("Role");
            ModelState.Remove("UserPassword");

            if (_context.Users.Any(u => u.UserLogin == item.UserLogin && u.Id != item.Id))
                ModelState.AddModelError("UserLogin", "Такой логин уже существует");

            if (ModelState.IsValid)
            {
                var user = _context.Users.Find(id);
                if (user == null) return NotFound();

                user.UserLogin = item.UserLogin;
                user.UserName = item.UserName;
                user.IsActive = item.IsActive;
                user.RoleId = item.RoleId;
                if (!string.IsNullOrWhiteSpace(password))
                    user.UserPassword = PasswordHelper.Hash(password);

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
                ViewBag.Error = "Нельзя удалить запись: на неё ссылаются другие таблицы (корзина или заказы). Сначала удалите связанные записи.";
                return View("Delete", Load(id));
            }
        }

        private User? Load(int id)
        {
            return _context.Users.Include(x => x.Role).FirstOrDefault(x => x.Id == id);
        }

        private void FillLists()
        {
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "RoleName");
        }
    }
}
