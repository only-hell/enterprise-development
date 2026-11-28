using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VetClinic.Infrastructure.SqlServer.Configurations;

/// <summary>
/// Сопоставляет отметку обработки контракта с таблицей базы данных
/// </summary>
internal sealed class ProcessedContractConfiguration : IEntityTypeConfiguration<ProcessedContract>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ProcessedContract> builder)
    {
        builder.ToTable("ProcessedContracts");
        builder.HasKey(contract => contract.ExternalId);
        builder.Property(contract => contract.ExternalId).ValueGeneratedNever();
    }
}
