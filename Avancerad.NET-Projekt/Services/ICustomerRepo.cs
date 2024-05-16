using Avancerad.NET_Projekt_ClassLibrary.Models;

namespace Avancerad.NET_Projekt.Services
{
    public interface ICustomerRepo
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(int id);
        Task<Customer> GetCustomerWithAppointments(int id);
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task<Customer> DeleteAsync(int id);
        Task<int> GetCustomerAppointmentsWeek(int customerId, int year, int weekNumber);
    }
}
