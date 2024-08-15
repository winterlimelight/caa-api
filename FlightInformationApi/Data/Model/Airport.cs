using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightInformationApi.Data;

public class Airport
{
    public int AirportID {get;set;}

    /// <summary>Departure airport ICAO identifier</summary>
    [MaxLength(4)]
    public string Code { get; set; }

    /// <summary>Departure airport ICAO identifier</summary>
    public string Name { get; set; }
}