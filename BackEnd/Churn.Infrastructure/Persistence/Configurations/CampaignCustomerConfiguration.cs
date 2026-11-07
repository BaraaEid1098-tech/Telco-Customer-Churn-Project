using Churn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Churn.Infrastructure.Persistence.Configurations;

public sealed class CampaignCustomerConfiguration : IEntityTypeConfiguration<CampaignCustomer>
{
    public void Configure(EntityTypeBuilder<CampaignCustomer> builder)
    {
        builder.ToTable("CampaignCustomers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.CampaignOutcome).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.OfferDetails).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.CampaignId, x.CustomerId }).IsUnique();

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.CampaignCustomers)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.CampaignCustomers)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Prediction)
            .WithMany(x => x.CampaignCustomers)
            .HasForeignKey(x => x.PredictionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
