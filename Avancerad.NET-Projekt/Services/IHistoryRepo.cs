using Avancerad.NET_Projekt_ClassLibrary.Models;

namespace Avancerad.NET_Projekt.Services
{
    public interface IHistoryRepo
    {
        Task<IEnumerable<History>> GetAllAsync();
        Task<IEnumerable<History>> GetByIdAsync(int id);
        Task<History> CreateAsync(History history);
    }
}
