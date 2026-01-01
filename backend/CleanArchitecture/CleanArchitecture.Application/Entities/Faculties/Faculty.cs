namespace CleanArchitecture.Core.Entities.Faculties
{
    public class Faculty : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}