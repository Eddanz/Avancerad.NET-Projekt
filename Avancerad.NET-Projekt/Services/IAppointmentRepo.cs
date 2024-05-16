using Avancerad.NET_Projekt_ClassLibrary.Models;

namespace Avancerad.NET_Projekt.Services
{
    public interface IAppointmentRepo
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment> GetByIdAsync(int id);
        Task<Appointment> CreateAsync(Appointment appointment);
        Task<Appointment> UpdateAsync(Appointment appointment);
        Task<Appointment> DeleteAsync(int id);
        Task<IEnumerable<Customer>> GetCustomersWithAppointmentsInCurrentWeek();
    }
}
