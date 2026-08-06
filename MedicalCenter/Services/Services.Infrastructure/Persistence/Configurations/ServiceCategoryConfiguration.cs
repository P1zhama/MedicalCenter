using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Services.Infrastructure.Persistence.Entities;

namespace Services.Infrastructure.Persistence.Configurations;

public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategoryEntity>
{
    public static readonly Guid AnalysesId = Guid.Parse("0195a1a0-0000-4000-8000-000000000001");

    public static readonly Guid ConsultationId = Guid.Parse("0195a1a0-0000-4000-8000-000000000002");

    public static readonly Guid DiagnosticsId = Guid.Parse("0195a1a0-0000-4000-8000-000000000003");

    public void Configure(EntityTypeBuilder<ServiceCategoryEntity> builder)
    {
        builder.ToTable("ServiceCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100)
            .UseCollation("Latin1_General_100_CI_AS");

        builder.Property(c => c.TimeSlotMinutes)
            .HasColumnName("time_slot_minutes")
            .IsRequired();

        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new ServiceCategoryEntity { Id = AnalysesId, Name = "Analyses", TimeSlotMinutes = 10 },
            new ServiceCategoryEntity { Id = ConsultationId, Name = "Consultation", TimeSlotMinutes = 20 },
            new ServiceCategoryEntity { Id = DiagnosticsId, Name = "Diagnostics", TimeSlotMinutes = 30 });
    }
}
