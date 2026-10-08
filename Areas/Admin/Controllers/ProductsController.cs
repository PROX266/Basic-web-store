using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageSize = 2 * 1024 * 1024; // 2 МБ

        private readonly WebstoreContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(WebstoreContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View(_context.Products.Include(x => x.Category).OrderBy(x => x.Id).ToList());
        }

        public IActionResult Create()
        {
            FillLists();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("ProductName,Price,CategoryId")] Product item, IFormFile? imageFile)
        {
            ModelState.Remove("Category");

            string? newImage = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                var error = ValidateImage(imageFile);
                if (error != null) ModelState.AddModelError("ImagePath", error);
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                    newImage = SaveImage(imageFile);
                item.ImagePath = newImage;

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

            var item = _context.Products.Find(id);
            if (item == null) return NotFound();

            FillLists();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,ProductName,Price,CategoryId")] Product item, IFormFile? imageFile)
        {
            if (id != item.Id) return NotFound();

            ModelState.Remove("Category");

            var oldImage = _context.Products.AsNoTracking()
                .Where(p => p.Id == id).Select(p => p.ImagePath).FirstOrDefault();
            item.ImagePath = oldImage;

            bool hasNewFile = imageFile != null && imageFile.Length > 0;
            if (hasNewFile)
            {
                var error = ValidateImage(imageFile!);
                if (error != null) ModelState.AddModelError("ImagePath", error);
            }

            if (ModelState.IsValid)
            {
                if (hasNewFile)
                {
                    item.ImagePath = SaveImage(imageFile!);
                    DeleteImageFile(oldImage);
                }

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
                var imagePath = item.ImagePath;
                _context.Remove(item);
                _context.SaveChanges();
                DeleteImageFile(imagePath);
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

        private string? ValidateImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return "Допустимые форматы изображения: JPG, PNG, GIF, WEBP";
            if (file.Length > MaxImageSize)
                return "Размер изображения не должен превышать 2 МБ";
            if (!HasImageSignature(file, ext))
                return "Файл не является корректным изображением";
            return null;
        }

        // проверяем первые байты файла, а не только расширение
        private static bool HasImageSignature(IFormFile file, string ext)
        {
            var head = new byte[12];
            using var stream = file.OpenReadStream();
            int read = stream.Read(head, 0, head.Length);
            if (read < 4) return false;

            return ext switch
            {
                ".jpg" or ".jpeg" => head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF,
                ".png" => head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47,
                ".gif" => head[0] == 0x47 && head[1] == 0x49 && head[2] == 0x46,
                ".webp" => read >= 12 && head[0] == 0x52 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x46
                           && head[8] == 0x57 && head[9] == 0x45 && head[10] == 0x42 && head[11] == 0x50,
                _ => false
            };
        }

        private string SaveImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var folder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(folder);

            using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return "/uploads/products/" + fileName;
        }

        // удаляем только файлы, загруженные через админку (тестовые SVG в /images не трогаем)
        private void DeleteImageFile(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !imagePath.StartsWith("/uploads/products/")) return;

            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        private Product? Load(int id)
        {
            return _context.Products.Include(x => x.Category).FirstOrDefault(x => x.Id == id);
        }

        private void FillLists()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "NameCategories");
        }
    }
}
