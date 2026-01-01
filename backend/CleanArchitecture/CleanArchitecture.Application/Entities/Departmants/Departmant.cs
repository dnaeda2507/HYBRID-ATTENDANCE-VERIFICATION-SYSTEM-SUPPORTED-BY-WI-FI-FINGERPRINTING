using CleanArchitecture.Core.Entities.Faculties;

namespace CleanArchitecture.Core.Entities.Departmants
{
    public class Departmant : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }
    }
}