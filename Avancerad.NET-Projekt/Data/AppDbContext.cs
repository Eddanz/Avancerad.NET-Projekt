using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet <Appointment> Appointments { get; set; }
        public DbSet <Company> Companys { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<History> Historys { get; set; }
    }
}
