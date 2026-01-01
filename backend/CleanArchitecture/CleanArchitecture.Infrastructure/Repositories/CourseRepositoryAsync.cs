using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Courses;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class CourseRepositoryAsync : GenericRepositoryAsync<Course>, ICourseRepositoryAsync
    {
        private readonly DbSet<Course> _dbSet;

        public CourseRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbSet = dbContext.Set<Course>();
        }
    }
}