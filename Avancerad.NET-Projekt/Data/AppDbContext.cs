using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text;

namespace Avancerad.NET_Projekt.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet <Appointment> Appointments { get; set; }
        public DbSet <Company> Companys { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Tracker> Tracker { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            GetEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            GetEntities();
            return base.SaveChanges();
        }

        private void GetEntities()
        {
            var entitiesToTrack = new List<string> { "Customer", "Company", "Appointment" };

            var modifiedEntities = ChangeTracker.Entries()
                .Where(_ => _.State == EntityState.Added || _.State == EntityState.Deleted || _.State == EntityState.Modified)
                .Where(_ => entitiesToTrack.Contains(_.Entity.GetType().Name))
                .ToList();

            foreach (var entity in modifiedEntities)
            {
                var trackerModel = new Tracker()
                {
                    ModelName = entity.Entity.GetType().Name,
                    ActionPerformed = entity.State.ToString(),
                    Timestamp = DateTime.UtcNow.ToLocalTime(),
                    Changes = GetChanges(entity)
                };
                Tracker.Add(trackerModel);
            }
        }

        private static string GetChanges(EntityEntry entityEntry)
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in entityEntry.OriginalValues.Properties)
            {
                var originalValue = entityEntry.OriginalValues[item];
                var currentValue = entityEntry.CurrentValues[item];

                if (entityEntry.State == EntityState.Added)
                    stringBuilder.AppendLine($"{item.Name}: {originalValue}");

                else if (entityEntry.State == EntityState.Modified)
                    stringBuilder.AppendLine($"{item.Name}: From {originalValue} to {currentValue}");

                else if (entityEntry.State == EntityState.Deleted)
                    stringBuilder.AppendLine($"{item.Name}: Deleted");
            }
            return stringBuilder.ToString();
        }
    }
}
