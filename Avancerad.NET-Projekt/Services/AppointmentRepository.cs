using Avancerad.NET_Projekt.Data;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Services
{
    public class AppointmentRepository : IAppointmentRepo
    {
        private readonly AppDbContext _context;
        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAsync(Appointment newEntity)
        {
            var result = await _context.Appointments.AddAsync(newEntity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Appointment> DeleteAsync(int id)
        {
            var result = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == id);
            if (result != null)
            {
                _context.Appointments.Remove(result);
                await _context.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments.ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _context.Appointments.SingleOrDefaultAsync(a => a.AppointmentID == id);
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithAppointmentsInCurrentWeek()
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            var customers = await _context.Customers
                .Where(c => c.Appointments.Any(a => a.AttendDate >= startOfWeek && a.AttendDate <= endOfWeek))
                .ToListAsync();

            return customers.Distinct();
        }

        public async Task<Appointment> UpdateAsync(Appointment entity)
        {
            var result = await _context.Appointments.SingleOrDefaultAsync(a => a.AppointmentID == entity.AppointmentID);
            if (result != null)
            {
                result.AttendDate = entity.AttendDate;

                await _context.SaveChangesAsync();
                return result;
            }
            return null;
        }
    }
}
