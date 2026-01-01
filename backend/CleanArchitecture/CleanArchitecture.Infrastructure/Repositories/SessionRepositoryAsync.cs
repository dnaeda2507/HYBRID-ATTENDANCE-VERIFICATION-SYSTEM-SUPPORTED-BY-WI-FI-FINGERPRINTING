using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Sessions;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class SessionRepositoryAsync : GenericRepositoryAsync<Session>, ISessionRepositoryAsync
    {
        private readonly DbSet<Session> _dbSet;

        public SessionRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbSet = dbContext.Set<Session>();
        }
    }
}