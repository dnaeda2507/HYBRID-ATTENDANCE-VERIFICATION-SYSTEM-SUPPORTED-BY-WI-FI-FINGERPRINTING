namespace CleanArchitecture.Core.Entities.Lectures
{
    public class Lecture : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}