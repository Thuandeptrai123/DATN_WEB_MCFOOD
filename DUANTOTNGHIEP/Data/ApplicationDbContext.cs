﻿using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DUANTOTNGHIEP.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<FoodType> FoodTypes { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboDetail> ComboDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<InvoiceHistory> InvoiceHistories { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId);

            builder.Entity<InvoiceItem>()
            .HasOne(ci => ci.Invoice)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.InvoiceId);

            builder.Entity<InvoiceHistory>()
            .HasOne(h => h.Invoice)
            .WithMany(i => i.Histories)
            .HasForeignKey(h => h.InvoiceId);

            // Cấu hình ApplicationRole
            builder.Entity<FoodType>(entity =>
            {
                // Cấu hình khóa chính
                entity.HasKey(r => r.FoodTypeId);

                // Cấu hình thuộc tính riêng của FoodType
                entity.Property(r => r.FoodTypeId).IsRequired();
                entity.Property(r => r.FoodTypeName)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(r => r.Description)
                      .HasMaxLength(255);

                // Cấu hình các thuộc tính kế thừa từ BaseModel
                entity.Property(r => r.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(r => r.UpdatedBy)
                      .HasMaxLength(255);

                entity.Property(r => r.CreatedDate)
                      .IsRequired();

                entity.Property(r => r.UpdatedDate)
                      .IsRequired();
            });
        }
    }
}
