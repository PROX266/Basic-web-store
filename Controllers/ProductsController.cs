using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class ProductsController : Controller
    {
        // Сколько товаров показывать на одной странице каталога
        private const int PageSize = 3;

        private readonly WebstoreContext _context;
        private readonly CatalogCache _cache;

        public ProductsController(WebstoreContext context, CatalogCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult Index(string? search, int? categoryId, string? sort, int page = 1)
        {
            search = search?.Trim();
            sort = string.IsNullOrEmpty(sort) ? "name_asc" : sort;

            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category);

            // поиск по названию
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search));

            // фильтр по категории
            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // сортировка (Id добавлен вторым ключом, чтобы порядок на страницах был стабильным)
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(p => p.ProductName).ThenBy(p => p.Id),
                "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
                "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
                _ => query.OrderBy(p => p.ProductName).ThenBy(p => p.Id)
            };

            int total = query.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Min(Math.Max(page, 1), totalPages);

            var model = new CatalogViewModel
            {
                Products = query.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                Categories = _cache.GetCategories(),
                Search = search,
                CategoryId = categoryId,
                Sort = sort,
                Page = page,
                PageSize = PageSize,
                TotalItems = total
            };

            return View(model);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var product = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefault(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}
