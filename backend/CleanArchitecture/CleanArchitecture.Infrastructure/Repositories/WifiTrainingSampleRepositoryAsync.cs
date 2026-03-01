using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class WifiTrainingSampleRepositoryAsync : GenericRepositoryAsync<WifiTrainingSample>, IWifiTrainingSampleRepositoryAsync
    {
        private readonly DbSet<WifiTrainingSample> _dbSet;

        public WifiTrainingSampleRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbSet = dbContext.Set<WifiTrainingSample>();
        }
    }
}
