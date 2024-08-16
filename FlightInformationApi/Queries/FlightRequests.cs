
using System;

/// <summary>Options used when searching for flights</summary>
public class FlightSearchOptions
{
    /// <summary>Airline to search for</summary>
    public string Airline {get;set;}
    /// <summary>Airport to search for (by name or ICAO code)</summary>
    public string Airport {get;set;}
    /// <summary>Limit flights to those departing after this date</summary>
    public DateTimeOffset? FromDate {get;set;}
    /// <summary>Limit flights to those arriving before this date</summary>
    public DateTimeOffset? ToDate {get;set;}
}