using System;

namespace FlightInformationApi.Queries;

public class FlightResponse
{
    /// <summary>Database ID for the flight</summary>
    public int FlightID { get; set; }

    /// <summary>Airline flight number</summary>
    public string FlightNumber { get; set; }

    /// <summary>Name of airline operating flight</summary>
    public string Airline { get; set; }

    /// <summary>Departure airport</summary>
    public string DepartureAirport { get; set; }

    /// <summary>Arrival airport</summary>
    public string ArrivalAirport { get; set; }

    /// <summary>UTC departure time</summary>
    public DateTimeOffset DepartureTime { get; set; }

    /// <summary>UTC arrival time</summary>
    public DateTimeOffset ArrivalTime { get; set; }

    /// <summary>Status of flight</summary>
    public Data.FlightStatus Status { get; set; }

    ///<summary>Field used to detect concurrent changes</summary>
    public Guid Version { get; set; }
}