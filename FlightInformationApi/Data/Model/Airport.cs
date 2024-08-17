using System.ComponentModel.DataAnnotations;

namespace FlightInformationApi.Data;

/// <summary>Database model for an Airport</summary>
public class Airport
{
    public int AirportID {get;set;}

    /// <summary>Departure airport ICAO identifier</summary>
    [MaxLength(4)]
    public string Code { get; set; }

    /// <summary>Departure airport ICAO identifier</summary>
    public string Name { get; set; }
}