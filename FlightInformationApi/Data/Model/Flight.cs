using System;
using System.ComponentModel.DataAnnotations;

namespace FlightInformationApi.Data;

public enum FlightStatus
{
    /// <summary>Flight is scheduled</summary>
    Scheduled = 1, 
    /// <summary>Flight is delayed</summary>
    Delayed, 
    /// <summary>Flight was cancelled</summary>
    Cancelled, 
    /// <summary>Flight is enroute</summary>
    InAir, 
    /// <summary>Flight has landed</summary>
    Landed
}

/// <summary>Database model for a Flight</summary>
public class Flight
{
    public int FlightID { get; set; }

    /// <summary>Airline flight number</summary>
    /// <remarks>ICAO indicates "Flight Identification now stipulates: not exceeding 7 alphanumeric characters" (https://www.icao.int/APAC/Meetings/2011_FPL_AM_TF4_Seminar/WP13.pdf)</remarks>
    [MaxLength(7)]
    public string FlightNumber { get; set; }

    /// <summary>Name of airline operating flight</summary>
    public string Airline { get; set; }

    /// <summary>Departure airport</summary>
    public Airport DepartureAirport { get; set; }

    /// <summary>Arrival airport</summary>
    public Airport ArrivalAirport { get; set; }

    /// <summary>UTC departure time</summary>
    public DateTimeOffset DepartureTime { get; set; }

    /// <summary>UTC arrival time</summary>
    public DateTimeOffset ArrivalTime { get; set; }

    /// <summary>Status of flight</summary>
    public FlightStatus Status { get; set; }

    ///<summary>Field used to detect concurrent changes</summary>
    public Guid Version { get; set; }
}