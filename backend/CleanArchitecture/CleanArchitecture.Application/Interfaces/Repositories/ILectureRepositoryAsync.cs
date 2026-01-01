using System.Threading.Tasks;
using CleanArchitecture.Core.Entities.Lectures;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Application.Interfaces.Repositories
{
    public interface ILectureRepositoryAsync : IGenericRepositoryAsync<Lecture>
    {
        Task<Lecture> GetByCodeAsync(string code);
        Task<bool> IsCodeExists(string code);

    }
}