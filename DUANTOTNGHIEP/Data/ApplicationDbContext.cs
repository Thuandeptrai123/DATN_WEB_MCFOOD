using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
