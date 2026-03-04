using FinLedger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinLedger.Tests.WebApplicationFactory;

/// <summary>
/// Custom WebApplicationFactory that uses InMemory database for isolated integration tests.
/// </summary>
public class FinLedgerWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly string DatabaseName = "FinLedger_Test_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LedgerDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<LedgerDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));
        });
    }
}
