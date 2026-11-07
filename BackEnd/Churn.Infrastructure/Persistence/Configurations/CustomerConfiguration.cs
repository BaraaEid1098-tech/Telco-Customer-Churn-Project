using Churn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Churn.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerCode).HasColumnName("CustomerCode").HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.CustomerCode).IsUnique();

        builder.Property(x => x.Gender).HasMaxLength(10).IsRequired();
        builder.Property(x => x.MultipleLines).HasMaxLength(30).IsRequired();
        builder.Property(x => x.InternetService).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OnlineSecurity).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OnlineBackup).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DeviceProtection).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TechSupport).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StreamingTV).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StreamingMovies).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Contract).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();

        builder.Property(x => x.MonthlyCharges).HasPrecision(10, 2);
        builder.Property(x => x.TotalCharges).HasPrecision(10, 2);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
    }
}
