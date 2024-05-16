using Avancerad.NET_Projekt_ClassLibrary.Models;

namespace Avancerad.NET_Projekt.Services
{
    public interface ICompanyRepo
    {
        Task<IEnumerable<Company>> GetAllAsync();
        Task<Company> GetByIdAsync(int id);
        Task<Company> CreateAsync(Company company);
        Task<Company> UpdateAsync(Company company);
        Task<Company> DeleteAsync(int id);
        Task<IEnumerable<Appointment>> GetBookingsInDateRange(DateTime startDate, DateTime endDate);
    }
}
