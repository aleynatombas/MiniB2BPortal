using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniB2B.DataAccess.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Brand).HasMaxLength(100);
        builder.Property(x => x.ManufacturerCode).HasMaxLength(80);
        builder.Property(x => x.CustomCode1).HasMaxLength(80);
        builder.Property(x => x.CustomCode2).HasMaxLength(80);
        builder.Property(x => x.ImagePath).HasMaxLength(400);
        builder.Property(x => x.StockQuantity).IsRequired();
        builder.Property(x => x.CriticalStockLevel).IsRequired().HasDefaultValue(5);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.ProductCode).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Brand);
        builder.HasIndex(x => x.ManufacturerCode);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
