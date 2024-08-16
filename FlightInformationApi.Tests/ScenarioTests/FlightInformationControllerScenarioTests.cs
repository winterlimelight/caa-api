using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using FlightInformationApi.Commands;
using FlightInformationApi.Controllers;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using FlightInformationApi.Queries;

namespace FlightInformationApi.Tests.ScenarioTests;

/// <summary>Scenario tests covering FlightInformationController</summary>
public class FlightInformationControllerScenarioTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory = new CustomWebApplicationFactory<Program>();
    private readonly Mock<ILogger<FlightInformationController>> _logger = new();

    void IDisposable.Dispose()
    {
        _factory?.Dispose();
    }

    [Fact]
    public async Task FlightInformation_Crud()
    {
        HttpClient client = CreateClientWithLogger();

        // verify no flights loaded
        var response = await client.GetAsync("/api/flights");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await TestHelpers.ReadBody<FlightResponse[]>(response));

        // create a flight
        var request = new SetFlightCommand
        {
            FlightNumber = "ANZ179M",
            Airline = "Air New Zealand",
            DepartureAirport = "NZPM",
            ArrivalAirport = "NZCH",
            DepartureTime = new DateTimeOffset(2024, 8, 16, 7, 5, 0, TimeSpan.Zero),
            ArrivalTime = new DateTimeOffset(2024, 8, 16, 8, 25, 0, TimeSpan.Zero),
            Status = FlightStatus.Scheduled
        };
        response = await client.PostAsync("/api/flights", TestHelpers.ToJsonBody(request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // fetch the flight
        response = await client.GetAsync(response.Headers.Location.AbsolutePath);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flight = await TestHelpers.ReadBody<FlightResponse>(response);
        Assert.Equal("ANZ179M", flight.FlightNumber);
        Assert.Equal(FlightStatus.Scheduled, flight.Status);

        // update the flight
        flight.Status = FlightStatus.Landed;
        response = await client.PutAsync("/api/flights/" + flight.FlightID, TestHelpers.ToJsonBody(flight));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // fetch all flights
        response = await client.GetAsync("/api/flights");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var allFlights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Equal("ANZ179M", allFlights.Single().FlightNumber);
        Assert.Equal(FlightStatus.Landed, allFlights.Single().Status);

        // TODO delete the flight


    }

    [Fact]
    public async Task FlightInformation_Conflict()
    {
        HttpClient client = CreateClientWithLogger();

        // verify no flights loaded
        var response = await client.GetAsync("/api/flights");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await TestHelpers.ReadBody<FlightResponse[]>(response));

        // create a flight
        var request = new SetFlightCommand
        {
            FlightNumber = "ANZ179M",
            Airline = "Air New Zealand",
            DepartureAirport = "NZPM",
            ArrivalAirport = "NZCH",
            DepartureTime = new DateTimeOffset(2024, 8, 16, 7, 5, 0, TimeSpan.Zero),
            ArrivalTime = new DateTimeOffset(2024, 8, 16, 8, 25, 0, TimeSpan.Zero),
            Status = FlightStatus.Scheduled
        };
        response = await client.PostAsync("/api/flights", TestHelpers.ToJsonBody(request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // fetch the flight
        response = await client.GetAsync(response.Headers.Location.AbsolutePath);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flight1 = await TestHelpers.ReadBody<FlightResponse>(response);
        Assert.Equal("ANZ179M", flight1.FlightNumber);
        Assert.Equal(FlightStatus.Scheduled, flight1.Status);

        // second user fetches the flight
        response = await client.GetAsync("/api/flights");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flight2 = (await TestHelpers.ReadBody<FlightResponse[]>(response)).Single();

        // second user updates the flight
        flight2.Status = FlightStatus.Delayed;
        response = await client.PutAsync("/api/flights/" + flight2.FlightID, TestHelpers.ToJsonBody(flight2));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // first user tries to update the flight
        flight1.Status = FlightStatus.InAir;
        response = await client.PutAsync("/api/flights/" + flight1.FlightID, TestHelpers.ToJsonBody(flight1));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // database refelcts second user's change
        response = await client.GetAsync("/api/flights");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedFlight = (await TestHelpers.ReadBody<FlightResponse[]>(response)).Single();
        Assert.Equal(FlightStatus.Delayed, updatedFlight.Status);
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
}