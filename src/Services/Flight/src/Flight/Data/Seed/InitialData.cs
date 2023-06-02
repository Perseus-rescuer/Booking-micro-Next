namespace Flight.Data.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using Aircrafts.Models;
using Aircrafts.Models.ValueObjects;
using Airports.Models;
using Flight.Aircrafts.ValueObjects;
using Flights.Models;
using MassTransit;
using Seats.Models;

public static class InitialData
{
    public static List<Airport> Airports { get; }
    public static List<Aircraft> Aircrafts { get; }
    public static List<Seat> Seats { get; }
    public static List<Flight> Flights { get; }


    static InitialData()
    {
        Airports = new List<Airport>
        {
            Airport.Create(new Guid("3c5c0000-97c6-fc34-a0cb-08db322230c8"), "Lisbon International Airport", "LIS", "12988"),
            Airport.Create(new Guid("3c5c0000-97c6-fc34-fc3c-08db322230c8"), "Sao Paulo International Airport", "BRZ", "11200")
        };

        Aircrafts = new List<Aircraft>
        {
            Aircraft.Create(AircraftId.Of(new Guid("3c5c0000-97c6-fc34-fcd3-08db322230c8")), Name.Of("Boeing 737"), Model.Of("B737"), ManufacturingYear.Of(2005)),
            Aircraft.Create(AircraftId.Of(new Guid("3c5c0000-97c6-fc34-2e04-08db322230c9")), Name.Of("Airbus 300"), Model.Of("A300"), ManufacturingYear.Of(2000)),
            Aircraft.Create(AircraftId.Of(new Guid("3c5c0000-97c6-fc34-2e11-08db322230c9")), Name.Of("Airbus 320"), Model.Of("A320"), ManufacturingYear.Of(2003))
        };


        Flights = new List<Flight>
        {
            Flight.Create(new Guid("3c5c0000-97c6-fc34-2eb9-08db322230c9"), "BD467", Aircrafts.First().Id, Airports.First().Id, new DateTime(2022, 1, 31, 12, 0, 0),
                new DateTime(2022, 1, 31, 14, 0, 0),
                Airports.Last().Id, 120m,
                new DateTime(2022, 1, 31), global::Flight.Flights.Enums.FlightStatus.Completed,
                8000)
        };

        Seats = new List<Seat>
        {
            Seat.Create(NewId.NextGuid(), "12A", global::Flight.Seats.Enums.SeatType.Window, global::Flight.Seats.Enums.SeatClass.Economy, Flights.First().Id),
            Seat.Create(NewId.NextGuid(), "12B", global::Flight.Seats.Enums.SeatType.Window, global::Flight.Seats.Enums.SeatClass.Economy, Flights.First().Id),
            Seat.Create(NewId.NextGuid(), "12C", global::Flight.Seats.Enums.SeatType.Middle, global::Flight.Seats.Enums.SeatClass.Economy, Flights.First().Id),
            Seat.Create(NewId.NextGuid(), "12D", global::Flight.Seats.Enums.SeatType.Middle, global::Flight.Seats.Enums.SeatClass.Economy, Flights.First().Id),
            Seat.Create(NewId.NextGuid(), "12E", global::Flight.Seats.Enums.SeatType.Aisle, global::Flight.Seats.Enums.SeatClass.Economy, Flights.First().Id),
            Seat.Create(NewId.NextGuid(), "12F", global::Flight.Seats.Enums.SeatType.Aisle, global::Flight.Seats.Enums.SeatClass.Economy, Flights.First().Id)
        };
    }
}
