namespace RoadRegistry.Editor.Schema.Organizations
{
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OrganizationConfiguration : IEntityTypeConfiguration<OrganizationRecord>
    {
        public const string TableName = "Organization";

        public void Configure(EntityTypeBuilder<OrganizationRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.EditorSchema)
                .HasIndex(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired();
            b.Property(p => p.Code).IsRequired();
            b.Property(p => p.SortableCode).IsRequired();
            b.Property(p => p.DbaseRecord).IsRequired();
        }
    }
}
