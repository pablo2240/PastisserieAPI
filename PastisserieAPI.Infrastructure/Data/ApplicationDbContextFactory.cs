using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PastisserieAPI.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Connection string hardcoded para migraciones
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=PastisserieDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}