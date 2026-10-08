using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Validation;

namespace WebApplication1.Models;

public partial class Product
{
    public int Id { get; set; }

    [Display(Name = "Название товара")]
    [Required(ErrorMessage = "Введите название товара")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Название должно содержать от 3 до 150 символов")]
    [ProductNameFormat]
    [NoForbiddenWords("бесплатн", "халяв", "скидк", "распродаж", "дешев", "акционн")]
    [NotCategoryName(nameof(CategoryId))]
    [UniqueProductName(nameof(CategoryId), nameof(Id))]
    public string ProductName { get; set; } = null!;

    [Display(Name = "Цена")]
    [PriceRange(1, 10000000, 2)]
    public decimal Price { get; set; }

    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    [Display(Name = "Изображение")]
    [StringLength(300)]
    public string? ImagePath { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string ImageUrl => string.IsNullOrWhiteSpace(ImagePath) ? "/images/no-image.svg" : ImagePath;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
