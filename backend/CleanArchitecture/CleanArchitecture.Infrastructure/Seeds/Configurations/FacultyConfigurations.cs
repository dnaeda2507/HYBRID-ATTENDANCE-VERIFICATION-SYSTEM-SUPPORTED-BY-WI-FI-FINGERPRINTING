using CleanArchitecture.Core.Entities.Faculties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Seeds.Configurations
{
    public class FacultyConfigurations : IEntityTypeConfiguration<Faculty>
    {

        public const int EngineeringFacultyId = 1;

        public void Configure(EntityTypeBuilder<Faculty> builder)
        {
            builder.HasData(
                new Faculty
                {
                    Id = EngineeringFacultyId,
                    Name = "Engineering",
                    Description = "Engineering Faculty",
                }
            );
        }

    }
}