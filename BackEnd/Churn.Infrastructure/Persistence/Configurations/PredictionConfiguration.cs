using Churn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Churn.Infrastructure.Persistence.Configurations;

public sealed class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.ToTable("Predictions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChurnProbability).HasPrecision(5, 4);
        builder.Property(x => x.RiskLevel).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ModelVersion).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TopFactors).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Predictions)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CustomerId, x.CreatedAt });
    }
}
