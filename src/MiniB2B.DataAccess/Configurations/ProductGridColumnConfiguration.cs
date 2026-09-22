using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniB2B.DataAccess.Configurations;

public class ProductGridColumnConfiguration : IEntityTypeConfiguration<ProductGridColumn>
{
    public void Configure(EntityTypeBuilder<ProductGridColumn> builder)
    {
        builder.ToTable("ProductGridColumns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldName).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Header).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Width).HasMaxLength(20);
        builder.HasIndex(x => x.FieldName).IsUnique();
        builder.HasIndex(x => x.SortOrder);
    }
}
