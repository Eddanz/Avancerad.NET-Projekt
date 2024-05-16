using Avancerad.NET_Projekt.Data;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Services
{
    public class HistoryRepository : IHistoryRepo
    {
        private readonly AppDbContext _context;
        public HistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<History> CreateAsync(History history)
        {
            var result = await _context.Historys.AddAsync(history);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IEnumerable<History>> GetAllAsync()
        {
            return await _context.Historys.ToListAsync();
        }

        public async Task<IEnumerable<History>> GetByIdAsync(int id)
        {
            return await _context.Historys
            .Where(a => a.AppointmentID == id)
            .ToListAsync();
        }
    }
}
