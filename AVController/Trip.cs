using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public record class Trip
    {
        public Trip(int personId, long start, long destination, Vehicle? car = null)
        {
            PersonId = personId;
            Start = start;
            Destination = destination;
            Car = car;
        }
        public Vehicle? Car { get; set; }
        public int PersonId { get; }
        public long Start { get; }
        public long Destination { get; }
        public double TimeWaitingInS { get; set; } = 0;
        public override string ToString()
        {
            return $"PersonID: {PersonId}, VehicleID: {Car}, Start:{Start}, Dest:{Destination}, Seconds waiting: {TimeWaitingInS}";
        }
    }
}
