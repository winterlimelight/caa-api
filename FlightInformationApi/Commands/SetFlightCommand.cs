using System;
using System.ComponentModel.DataAnnotations;
using FlightInformationApi.Data;

public class SetFlightCommand
{
    /// <summary>Existing flight ID. 0 implies new flight.</summary>
    public int FlightID { get; internal set; }

    /// <summary>Airline flight number</summary>
    /// <remarks>ICAO indicates "Flight Identification now stipulates: not exceeding 7 alphanumeric characters" (https://www.icao.int/APAC/Meetings/2011_FPL_AM_TF4_Seminar/WP13.pdf)</remarks>
    /// <example>ANZ680</example>
    [RegularExpression(@"^[a-zA-Z0-9]{1,7}$", ErrorMessage = "Value must be 1 to 7 alphanumeric characters")]
    public string FlightNumber { get; set; }

    /// <summary>Name of airline operating flight</summary>
    /// <example>Air New Zealand</example>
    public string Airline { get; set; }

    /// <summary>Departure airport ICAO identifier</summary>
    /// <example>NZWN</example>
    [StringLength(4, ErrorMessage = "Value must be 4 character ICAO airport identifier")]
    public string DepartureAirport { get; set; }

    /// <summary>Arrival airport ICAO identifier</summary>
    /// <example>NZAA</example>
    [StringLength(4, ErrorMessage = "Value must be 4 character ICAO airport identifier")]
    public string ArrivalAirport { get; set; }

    /// <summary>UTC departure time</summary>
    /// <example>2024-08-15T20:20:00Z</example>
    public DateTimeOffset DepartureTime { get; set; }

    /// <summary>UTC arrival time</summary>
    /// <example>2024-08-15T21:25:00Z</example>
    public DateTimeOffset ArrivalTime { get; set; }

    /// <summary>Status of the flight</summary>
    /// <example>InAir</example>
    public FlightStatus Status { get; set; }

    ///<summary>Field used to detect concurrent changes. Not required when creating flight.</summary>
    public Guid Version { get; set; }
}