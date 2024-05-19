using Avancerad.NET_Projekt.Data;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Services
{
    public class CompanyRepository : ICompanyRepo
    {
        private readonly AppDbContext _context;
        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Company> CreateAsync(Company newEntity)
        {
            var result = await _context.Companys.AddAsync(newEntity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Company> DeleteAsync(int id)
        {
            var result = await _context.Companys.FirstOrDefaultAsync(c => c.CompanyID == id);
            if (result != null)
            {
                _context.Companys.Remove(result);
                await _context.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companys.ToListAsync();
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            return await _context.Companys.SingleOrDefaultAsync(c => c.CompanyID == id);
        }

        public async Task<Company> UpdateAsync(Company entity)
        {
            var result = await _context.Companys.SingleOrDefaultAsync(c => c.CompanyID == entity.CompanyID);
            if (result != null)
            {
                result.CompanyName = entity.CompanyName;

                await _context.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<Appointment>> GetBookingsInDateRange(DateTime startDate, DateTime endDate)
        {
            // Hämta alla bokningar som ligger inom det angivna datumintervallet
            var bookings = await _context.Appointments
                .Where(a => a.AttendDate >= startDate && a.AttendDate <= endDate)
                .ToListAsync();

            return bookings;
        }
    }
}
