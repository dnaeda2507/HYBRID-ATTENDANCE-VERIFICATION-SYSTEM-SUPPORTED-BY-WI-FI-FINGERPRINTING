using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class LectureRepositoryAsync : GenericRepositoryAsync<Lecture>, ILectureRepositoryAsync
    {
        private readonly DbSet<Lecture> _lectures;
        public LectureRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _lectures = dbContext.Set<Lecture>();
        }

        public async Task<Lecture> GetByCodeAsync(string code)
        {
            return await _lectures.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<bool> IsCodeExists(string code)
        {
            return await _lectures.AnyAsync(x => x.Code == code);
        }

    }
}