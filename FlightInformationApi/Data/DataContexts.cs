using Microsoft.EntityFrameworkCore;

namespace FlightInformationApi.Data;

/*
A little explanation of my approach here is required.

In a production system, where possible, I like to separate readers from writers.
This enables different connection strings to be used for readers and writers.

With different connection strings, different security can be applied, and there is more scope for scale in the future.
For instance it's common for a database to get read-replicas which having different connection strings for readers allows.

I also note that I've use Sqlite in-memory as Microsoft are steering people away from their InMemory solution:
"While it has become common to use the in-memory database for testing, this is discouraged" (https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory)
*/

public class ReadContext : FlightInformationContext
{
    public ReadContext(DbContextOptions<ReadContext> options) : base(options) { }
}

public class WriteContext : FlightInformationContext
{
    public WriteContext(DbContextOptions<WriteContext> options) : base(options) { }
}

public abstract class FlightInformationContext : DbContext
{
    public FlightInformationContext() {}
    public FlightInformationContext(DbContextOptions options) : base(options) { }

    public DbSet<Flight> Flights { get; set; }
    public DbSet<Airport> Airports { get; set; }
}