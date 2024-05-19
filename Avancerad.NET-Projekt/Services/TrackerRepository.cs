using Avancerad.NET_Projekt.Data;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Services
{
    public class TrackerRepository : ITrackerRepo
    {
        private readonly AppDbContext _context;

        public TrackerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tracker>> GetAllAsync()
        {
            return await _context.Tracker.ToListAsync();
        }
    }
}
