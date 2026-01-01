using CleanArchitecture.Core.Entities.Departmants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Seeds.Configurations
{
    public class DepartmantConfigurations : IEntityTypeConfiguration<Departmant>
    {
        public const int SoftwareEngineeringDepartmantId = 1;

        public void Configure(EntityTypeBuilder<Departmant> builder)
        {
            builder.HasData(
                new Departmant
                {
                    Id = SoftwareEngineeringDepartmantId,
                    Name = "Software Engineering",
                    Description = "Software Engineering Departmant",
                    FacultyId = FacultyConfigurations.EngineeringFacultyId
                }
            );
        }
    }
}