using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class WebstoreContext : DbContext
{
    public WebstoreContext()
    {
    }

    public WebstoreContext(DbContextOptions<WebstoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // строка подключения берётся из appsettings.json (см. Program.cs)
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:con");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__carts__3214EC27AD3E1EF7");

            entity.ToTable("carts");

            entity.HasIndex(e => e.UserId, "UQ__carts__CB9A1CDE9866DE9C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_carts_users");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cart_ite__3214EC27641D186D");

            entity.ToTable("cart_items");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CartId).HasColumnName("cart_ID");
            entity.Property(e => e.ProductId).HasColumnName("product_ID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_cart_items_carts");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cart_items_products");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3214EC2708AECE9A");

            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NameCategories)
                .HasMaxLength(100)
                .HasColumnName("name_categories");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orders__3214EC27B45E1FD7");

            entity.ToTable("orders");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orders_users");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__order_it__3214EC275618DE2D");

            entity.ToTable("order_items");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OrderId).HasColumnName("order_ID");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_ID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_order_items_orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_items_products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__products__3214EC278827C5B6");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("category_ID");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(300)
                .HasColumnName("image_path");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_products_categories");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3214EC279ACDE592");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__783254B19433F295").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3214EC27D6B3541A");

            entity.ToTable("users");

            entity.HasIndex(e => e.UserLogin, "UQ__users__9EA1B5AF49200C13").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoleId).HasColumnName("role_ID");
            entity.Property(e => e.UserLogin)
                .HasMaxLength(50)
                .HasColumnName("user_login");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(64)
                .HasColumnName("user_password");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
