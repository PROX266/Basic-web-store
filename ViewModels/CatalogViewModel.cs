using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class CatalogViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string Sort { get; set; } = "name_asc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
