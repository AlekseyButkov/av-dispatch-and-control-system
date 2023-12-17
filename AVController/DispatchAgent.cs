using AVGraphPrimitives;
using Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public class DispatchAgent
    {
        private int mFailedRoutes = 0;
        private GraphMap mWorldMap = new();
        private RouteFinder mRouteFinder;
        private TripRequestCoordinator mCoordinator;
        private WorldSimulation mSim;
        private List<Trip> mTripsInProgress = new();
        public DispatchAgent(WorldSimulation sim, TripRequestCoordinator tripRequester, RouteFinder routeFinder, GraphMap map)
        {
            mRouteFinder = routeFinder;
            mWorldMap = map;
            mCoordinator = tripRequester;
            mSim = sim;
            sim.TimeTick += OnTimeIncrement;
        }

        public void TestRoutes()
        {
            Stopwatch sw = new();
            while (true)
            {
                sw.Restart();
                mRouteFinder.TestRoutingBetweenRandomLocations();
                Console.WriteLine($"Found route in {sw.ElapsedMilliseconds}ms");
                Thread.Sleep(1000);
            }
        }

        public void TryARoute(WorldSimulation world, long destination)
        {
            var car = world.Vehicles[0];
            var start = car.Location;
            var path = mRouteFinder.FindRoute(start, destination, TypeOfOptimality.Time);
            if (path.Count != 0)
            {
                if (car.CanDoRoute(path))
                    car.BeginTrip(path);
                else
                    Console.WriteLine("Could not do route");
            }
        }

        /// <summary>
        /// Create new trips from incoming requests, send cars to pick them up.
        /// </summary>
        private void HandleNewRequests()
        {
            var request = mCoordinator.PopNextRequest();
            while (request != null)
            {
                // get an available vehicle or create a new one if there are none
                Vehicle car;
                var idleCars = mSim.Vehicles.Where(x => x.Status == VehicleStatus.Idle).ToList();
                if (idleCars.Count == 0)
                    car = mSim.RequestNewVehicle();
                else
                    car = idleCars[0];

                // Test whether full route is part of main connected graph, ignore the request if it is not
                var pathToRider = mRouteFinder.FindRoute(car.Location, request.Start, TypeOfOptimality.Time);
                if (pathToRider.Count == 0)
                {
                    // Car location does not need to be tested, cars start at already tested locations
                    mWorldMap.RemoveLocation(request.Start);
                    Console.WriteLine("Warning: Failed to find route when handling new trip request because no route was found");
                    return;
                }
                var requestedPath = mRouteFinder.FindRoute(request.Start, request.Destination, TypeOfOptimality.Time);
                if (requestedPath.Count == 0)
                {
                    if (!mRouteFinder.IsPartOfConnectedGraph(request.Start))
                        mWorldMap.RemoveLocation(request.Start);
                    if (!mRouteFinder.IsPartOfConnectedGraph(request.Destination))
                        mWorldMap.RemoveLocation(request.Destination);
                    return;
                }

                car.PickUpRider(pathToRider);
                request.Car = car;

                mTripsInProgress.Add(request);
            }
        }


        /// <summary>
        /// Deal with vehicles who have just picked up or delivered riders
        /// </summary>
        private void HandleInProgressTrips()
        {
            var trips = mTripsInProgress.Where(x => x.Car != null && x.Car.Status == VehicleStatus.AwaitingRider);
            foreach (var trip in trips)
            {
                if (trip.Car == null)
                    throw new Exception("Car cannot be null in a scheduled trip");
                var path = mRouteFinder.FindRoute(trip.Start, trip.Destination, TypeOfOptimality.Time);
                if (path.Count == 0)
                {
                    trip.Car.CancelCurrentRoute();
                    mTripsInProgress.Remove(trip);
                    mFailedRoutes++;
                    Console.WriteLine("Warning: Failed to find route when picking up rider");
                    return;
                }
                trip.Car.BeginTrip(path);
            }
        }

        public void OnTimeIncrement(object? sender,  EventArgs e)
        {
            HandleNewRequests();
            HandleInProgressTrips();
        }
    }
}
