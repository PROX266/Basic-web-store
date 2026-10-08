using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    // Кэширует список категорий: он нужен на каждой странице каталога и меняется редко
    public class CatalogCache
    {
        private const string CategoriesKey = "catalog_categories";

        private readonly IMemoryCache _cache;
        private readonly WebstoreContext _context;

        public CatalogCache(IMemoryCache cache, WebstoreContext context)
        {
            _cache = cache;
            _context = context;
        }

        public List<Category> GetCategories()
        {
            return _cache.GetOrCreate(CategoriesKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return _context.Categories.AsNoTracking().OrderBy(c => c.NameCategories).ToList();
            })!;
        }

        // вызывается из админки после любого изменения категорий
        public void ResetCategories() => _cache.Remove(CategoriesKey);
    }
}
