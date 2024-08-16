using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightInformationApi.Data;
using FlightInformationApi.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlightInformationApi.Tests.ContractTests;

/// <summary>Contract tests covering FlightInformationController.SearchFlights()</summary>
public class FlightInformationControllerSearchFlightTests : IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public FlightInformationControllerSearchFlightTests()
    {
        _factory = new CustomWebApplicationFactory<Program>();
    }

    void IDisposable.Dispose()
    {
        _factory?.Dispose();
    }

    private readonly string[] _compactFlights = [
        "1,ANZ991,NZPM,NZAA,2024-08-15T08:20:00Z,2024-08-15T09:20:00Z,Landed",
        "2,ANZ992,NZAA,NZWN,2024-08-16T09:00:00Z,2024-08-16T10:10:00Z,Delayed",
        "3,ANZ993,NZWN,NZCH,2024-08-16T18:00:00Z,2024-08-16T18:45:00Z,Scheduled",

        "4,QFA884,NZAA,NZDN,2024-08-15T04:50:00Z,2024-08-15T06:40:00Z,Landed",
        "5,QFA885,NZCH,NZWN,2024-08-16T06:00:00Z,2024-08-16T06:45:00Z,Cancelled",
        "6,QFA886,NZDN,NZAA,2024-08-17T07:30:00Z,2024-08-17T09:20:00Z,Scheduled",

        "7,SDA777,NZWN,NZWN,2024-08-16T07:00:00Z,2024-08-16T07:30:00Z,InAir",
    ];

    [Fact]
    public async Task FlightSearch_AirlineOnly()
    {
        await LoadFlights();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/search?airline=Air New Zealand");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Equal(3, flights.Length);
        AssertFlightNumbers(flights, "ANZ991","ANZ992","ANZ993");
    }

    [Fact]
    public async Task FlightSearch_AirportByNameOnly()
    {
        await LoadFlights();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/search?airport=Palmerston");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Single(flights);
        AssertFlightNumbers(flights, "ANZ991");
    }

    [Fact]
    public async Task FlightSearch_AirportByCodeOnly()
    {
        await LoadFlights();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/search?airport=NZAA");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Equal(4, flights.Length);
        AssertFlightNumbers(flights, "ANZ991", "ANZ992", "QFA884", "QFA886");
    }

    [Fact]
    public async Task FlightSearch_FromDateOnly()
    {
        await LoadFlights();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/search?fromDate=2024-08-16T12:00:00Z");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Equal(2, flights.Length);
        AssertFlightNumbers(flights, "ANZ993", "QFA886");
    }

    [Fact]
    public async Task FlightSearch_ToDateOnly()
    {
        await LoadFlights();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights/search?toDate=2024-08-16T12:00:00Z");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var flights = await TestHelpers.ReadBody<FlightResponse[]>(response);
        Assert.Equal(5, flights.Length);
        AssertFlightNumbers(flights, "ANZ991", "ANZ992", "QFA884", "QFA885", "SDA777");
    }

    // todo muliple filters: esp both dates
    // todo param validation esp dates.

    private async Task LoadFlights()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WriteContext>();

        List<Airport> airports = await db.Airports.ToListAsync();
        var airlines = new Dictionary<string,string> {
            {"ANZ", "Air New Zealand"},
            {"QFA", "Qantas"},
            {"SDA", "Sounds Air"},
        };

        var flights = new List<Flight>();
        foreach(string line in _compactFlights)
        {
            string[]fields = line.Split(',');
            flights.Add(new Flight {
                FlightID = int.Parse(fields[0]),
                FlightNumber = fields[1],
                Airline = airlines[fields[1].Substring(0,3)],
                DepartureAirport = airports.Single(a => a.Code == fields[2]),
                ArrivalAirport = airports.Single(a => a.Code == fields[3]),
                DepartureTime = DateTimeOffset.Parse(fields[4]),
                ArrivalTime = DateTimeOffset.Parse(fields[5]),
                Status = Enum.Parse<FlightStatus>(fields[6])
            });
        }

        db.Flights.AddRange(flights);
        await db.SaveChangesAsync();
    }

    private void AssertFlightNumbers(FlightResponse[] flights, params string[] flightNumbers)
    {
        var foundFlights = flights.Select(f => f.FlightNumber).ToHashSet();
        Assert.True(foundFlights.SetEquals(flightNumbers));
    }
}