using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SistemaTurnos.Infrastructure.Persistence;

public class SistemaTurnosDbContextFactory : IDesignTimeDbContextFactory<SistemaTurnosDbContext>
{
    public SistemaTurnosDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SistemaTurnos.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<SistemaTurnosDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SistemaTurnosDbContext(optionsBuilder.Options);
    }
}