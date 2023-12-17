using AVGraphPrimitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public class TripRequestCoordinator
    {
        private Random mRandom = new();
        private List<Trip> mRequests = new();
        private GraphMap mMap;
        private double mTimeStepInS;
        private double mTicksPerDay;
        private double mRequestsPerTStep;
        private int mPopulation;

        /// <summary>
        /// Implements a FIFO queue for trip requests and generates new requests with time
        /// </summary>
        /// <param name="mSim">World simulation to subscribe time increments to</param>
        /// <param name="map">World map</param>
        /// <param name="tickIncrememntInSeconds">How many seconds is in a single time tick</param>
        /// <param name="meanRequestsPerHour">Average number of requests to generate per hour</param>
        public TripRequestCoordinator(WorldSimulation sim, GraphMap map, double tickIncrememntInSeconds, int meanRequestsPerHour, int population)
        {
            mMap = map;
            mTimeStepInS = tickIncrememntInSeconds;
            mTicksPerDay = (24 * 3600) / tickIncrememntInSeconds;
            mRequestsPerTStep = ((double)meanRequestsPerHour / 3600) * tickIncrememntInSeconds;
            mPopulation = population;
            sim.TimeTick += OnTimeIncrement;
        }

        public int NumPendingRequests {  get { return mRequests.Count; } }

        /// <summary>
        /// Pops the oldest trip request from pending requests
        /// </summary>
        /// <returns>A TripRequest if one exists, null otherwise</returns>
        public Trip? PopNextRequest()
        {
            if (mRequests.Count == 0)
                return null;
            var request = mRequests.First();
            mRequests.Remove(request);
            return request;
        }

        /// <summary>
        /// Create a single new trip request
        /// </summary>
        public Trip CreateNewRequest()
        {
            var start = mMap.GetRandomValidLocation();
            var destination = mMap.GetRandomValidLocation();
            var trip = new Trip(mRandom.Next(mPopulation), start, destination);
            return trip;
        }

        /// <summary>
        /// Potentially generates new requests, with the chance based on the average number of requests per time step.
        /// </summary>
        private void GenerateNewRequests()
        {
            var remainingRequestChance = mRequestsPerTStep;
            while (remainingRequestChance > 0)
            {
                // we want to generate a number between 0 and 1, but simpler to use int Random and treat it as a percentile rather than
                // doing random float generation
                var percentileRoll = mRandom.Next(100);
                if (percentileRoll < remainingRequestChance * 100)
                {
                    var request = CreateNewRequest();
                    mRequests.Add(request);
                    Console.WriteLine($"Generated new trip request: {request}");
                }
                remainingRequestChance--;
            }
        }

        private void OnTimeIncrement(object? sender, EventArgs e)
        {
            GenerateNewRequests();
        }
    }
}
