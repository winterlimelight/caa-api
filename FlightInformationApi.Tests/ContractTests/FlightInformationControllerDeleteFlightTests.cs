using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using FlightInformationApi.Controllers;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FlightInformationApi.Tests.ContractTests;

/// <summary>Contract tests covering FlightInformationController.DeleteFlight()</summary>
public class FlightInformationControllerDeleteFlightTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory = new CustomWebApplicationFactory<Program>();
    private readonly Mock<ILogger<FlightInformationController>> _logger = new();

    void IDisposable.Dispose()
    {
        _factory?.Dispose();
    }

    [Fact]
    public async Task DeleteFlight_Success()
    {
        HttpClient client = CreateClientWithLogger();
        Flight originalFlight = await CreateTestFlight();

        var response = await client.DeleteAsync($"/api/flights/{originalFlight.FlightID}?version={originalFlight.Version}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(GetFlightFromDatabase());

        VerifyLogger(LogLevel.Trace, "DeleteFlight()", Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteFlight_NotFoundFlightID()
    {
        HttpClient client = CreateClientWithLogger();
        var response = await client.DeleteAsync($"/api/flights/15?version={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteFlight_InvalidFlightID(int flightID)
    {
        HttpClient client = CreateClientWithLogger();
        var response = await client.DeleteAsync($"/api/flights/{flightID}?version={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"Delete flight request must have positive FlightID and non-empty Version", body);
    }

    [Fact]
    public async Task DeleteFlight_InvalidVersion()
    {
        HttpClient client = CreateClientWithLogger();
        var response = await client.DeleteAsync($"/api/flights/1?version={Guid.Empty}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"Delete flight request must have positive FlightID and non-empty Version", body);
    }

    [Fact]
    public async Task DeleteFlight_WrongVersion()
    {
        HttpClient client = CreateClientWithLogger();
        Flight originalFlight = await CreateTestFlight();
        var response = await client.DeleteAsync($"/api/flights/{originalFlight.FlightID}?version={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private HttpClient CreateClientWithLogger()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_logger.Object);
            });
        }).CreateClient();
    }

    private async Task<Flight> CreateTestFlight()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();

            Flight testFlight1 = new Flight
            {
                FlightID = 1,
                FlightNumber = "TSC236",
                Airline = "Air Transat",
                DepartureAirport = new Airport { Code = "CYYZ", Name = "Toronto" },
                ArrivalAirport = new Airport { Code = "LPPT", Name = "Lisbon" },
                DepartureTime = new DateTimeOffset(2001, 8, 24, 0, 52, 0, TimeSpan.Zero),
                ArrivalTime = new DateTimeOffset(2001, 8, 24, 8, 0, 0, TimeSpan.Zero),
                Status = FlightStatus.InAir,
                Version = Guid.NewGuid()
            };

            db.Flights.AddRange(testFlight1);
            db.Airports.Add(new Airport { Code = "LPLA", Name = "Lajes Airport" });
            await db.SaveChangesAsync();
            return testFlight1;
        }
    }

    // https://adamstorr.co.uk/blog/mocking-ilogger-with-moq/
    private void VerifyLogger(LogLevel level, string startsWith, Times times)
    {
        _logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith(startsWith)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ), times);
    }

    private Flight GetFlightFromDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
        return db.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .SingleOrDefault();
    }
}