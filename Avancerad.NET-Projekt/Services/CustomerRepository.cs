using Avancerad.NET_Projekt.Data;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Avancerad.NET_Projekt.Methods;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Services
{
    public class CustomerRepository : ICustomerRepo
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            var result = await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Customer> DeleteAsync(int id)
        {
            var result = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
            if (result != null)
            {
                _context.Customers.Remove(result);
                await _context.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public IQueryable<Customer> GetAllAsyncQuery()
        {
            return _context.Customers.AsQueryable();
        }

        public async Task<Customer> GetCustomerWithAppointments(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            return customer;
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers.SingleOrDefaultAsync(c => c.CustomerID == id);
        }

        public async Task<Customer> UpdateAsync(Customer entity)
        {
            var result = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == entity.CustomerID);
            if (result != null)
            {
                result.FirstName = entity.FirstName;
                result.LastName = entity.LastName;
                result.Phone = entity.Phone;

                await _context.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<int> GetCustomerAppointmentsWeek(int customerId, int year, int weekNumber)
        {
            // Calculate start and end dates for the specified week
            DateTime startOfWeek = DateHelper.FirstDateOfWeek(year, weekNumber);
            DateTime endOfWeek = startOfWeek.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

            // Retrieve appointments for the specified customer within the specified week,
            // excluding appointments that are flagged as deleted
            int appointmentsCount = await _context.Appointments
                .Where(a => a.CustomerID == customerId &&
                            a.AttendDate >= startOfWeek &&
                            a.AttendDate <= endOfWeek)
                .CountAsync();

            return appointmentsCount;
        }
    }
}
