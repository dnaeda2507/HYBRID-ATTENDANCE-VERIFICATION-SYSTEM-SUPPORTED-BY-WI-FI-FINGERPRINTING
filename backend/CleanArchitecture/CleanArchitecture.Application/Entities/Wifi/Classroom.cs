using System.Collections.Generic;
using CleanArchitecture.Core.Entities.Courses;

namespace CleanArchitecture.Core.Entities
{
    public class Classroom : AuditableBaseEntity
    {
        public string Name { get; set; }       // 205, 206 vb.
        public string Building { get; set; }   // Mühendislik, Fen vb.

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}