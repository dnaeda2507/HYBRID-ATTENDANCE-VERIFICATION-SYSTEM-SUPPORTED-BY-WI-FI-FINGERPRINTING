using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class StudentWifiScanRepositoryAsync : GenericRepositoryAsync<StudentWifiScan>, IStudentWifiScanRepositoryAsync
    {
        private readonly DbSet<StudentWifiScan> _dbSet;

        public StudentWifiScanRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbSet = dbContext.Set<StudentWifiScan>();
        }
    }
}
