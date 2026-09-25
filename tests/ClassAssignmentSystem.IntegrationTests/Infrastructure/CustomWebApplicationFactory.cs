using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClassAssignmentSystem.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IntegrationTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemory",
                ["Jwt:Secret"] = "IntegrationTestSecretKey_Minimum32Chars!",
                ["Jwt:Issuer"] = "ClassAssignmentSystem",
                ["Jwt:Audience"] = "ClassAssignmentSystem",
                ["Jwt:ExpiryMinutes"] = "120",
                ["BlobStorage:SubmissionsContainer"] = "assignment-submissions",
                ["BlobStorage:AssignmentMaterialsContainer"] = "assignment-materials",
                ["BlobStorage:MaxFileSizeBytes"] = "10485760",
                ["BlobStorage:SasTokenExpiryMinutes"] = "15"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();


            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IBlobStorageService>();
            services.AddSingleton<IBlobStorageService, FakeBlobStorageService>();
        });
    }
}
