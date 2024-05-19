using Avancerad.NET_Projekt_ClassLibrary.Models;

namespace Avancerad.NET_Projekt.Services
{
    public interface ITrackerRepo
    {
        Task<List<Tracker>> GetAllAsync();
    }
}
