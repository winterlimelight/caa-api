using System;
using System.ComponentModel.DataAnnotations;

namespace FlightInformationApi.Commands;

public class DeleteFlightCommand
{
    /// <summary>Flight ID</summary>
    [RequiresValue]
    [Range(1, int.MaxValue)]
    public int FlightID { get; internal set; }

    ///<summary>Field used to detect concurrent changes. Not required when creating flight.</summary>
    [RequiresValue]
    public Guid Version { get; set; }
}