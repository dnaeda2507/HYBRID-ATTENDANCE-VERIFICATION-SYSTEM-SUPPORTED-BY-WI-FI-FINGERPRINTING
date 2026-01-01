using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class AttendanceRepositoryAsync : GenericRepositoryAsync<Attendance>, IAttendanceRepositoryAsync
    {
        private readonly DbSet<Attendance> _dbSet;

        public AttendanceRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbSet = dbContext.Set<Attendance>();
        }
    }
}