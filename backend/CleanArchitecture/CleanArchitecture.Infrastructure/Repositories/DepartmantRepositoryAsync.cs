using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Departmants;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class DepartmantRepositoryAsync : GenericRepositoryAsync<Departmant>, IDepartmantRepositoryAsync
    {
        private readonly DbSet<Departmant> _dbSet;

        public DepartmantRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbSet = dbContext.Set<Departmant>();
        }
    }
}