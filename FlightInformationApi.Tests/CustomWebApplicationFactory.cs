using System;
using System.Data.Common;
using System.Linq;
using FlightInformationApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Custom factory for integration tests</summary>
/// <remarks>see https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0</remarks>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WriteContext>)));
            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ReadContext>)));

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
            services.Remove(dbConnectionDescriptor);

            services.AddSingleton<DbConnection>(container =>
            {
                // If two or more distinct but shareable in-memory databases are needed in a single process, 
                // then the mode=memory query parameter can be used - https://www.sqlite.org/inmemorydb.html
                var connection = new SqliteConnection($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared");                
                connection.Open();
                return connection;
            });

            services.AddDbContext<WriteContext>((container, options) => options.UseSqlite(container.GetRequiredService<DbConnection>()));
            services.AddDbContext<ReadContext>((container, options) => options.UseSqlite(container.GetRequiredService<DbConnection>()));
        });

        builder.UseEnvironment("Development");
    }
}
