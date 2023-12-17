using AVGraphPrimitives;
using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public enum VehicleStatus
    {
        Idle,
        Charging,
        Transporting,
        PickingUpRider,
        AwaitingRider
    }


    /// <summary>
    /// Simulated vehicle class
    /// </summary>
    public class Vehicle
    {
        private const double GOOD_METERS_PER_CHARGE = 6500;
        private const double MAX_CHARGE = 100;

        public static double sTotalMetersDriven = 0;
        public static double sTotalChargeUsed = 0;
        public static int sRidersTransported = 0;

        private VehicleStatus mStatus;
        private double mCharge;
        private double mMetersPerUnitOfCharge;
        private double mDistanceRemainingOnCurrentHop;
        private long mCurrentDestination = -1;
        private long mLastLocation = -1;
        private int mIndexOnPath;
        private string mTripLog = "";
        private GraphMap mMap;
        private WorldSimulation mSim;
        private List<GraphEdge> mCurrentPath = new();

        public static double TotalDistanceKm { get { return sTotalMetersDriven / 1000; } }
        public static double TotalChargeUsed { get { return sTotalChargeUsed; } }
        public static int RidersTransported { get { return sRidersTransported; } }

        public int Id { get; }
        public double RemainingCharge
        { get { return mCharge; } }
        public VehicleStatus Status
        { get { return mStatus; } }
        public long Location
        { get { return mLastLocation; } }
        public Vehicle(int id,
            WorldSimulation sim,
            GraphMap worldMap,
            long startingLocationId,
            VehicleStatus status = VehicleStatus.Idle,
            double startingCharge = MAX_CHARGE,
            double distancePerUnitOfCharge = GOOD_METERS_PER_CHARGE)
        {
            Id = id;
            mMap = worldMap;
            mMetersPerUnitOfCharge = distancePerUnitOfCharge;
            mLastLocation = startingLocationId;
            mCharge = startingCharge;
            mSim = sim;
            mStatus = status;
            mSim.TimeTick += OnTimeIncrement;
        }

        private double GetTotalPathLength(List<GraphEdge> path)
        {
            double total = 0;
            for (int i = 0; i < path.Count; i++)
            {
                total += path[i].Length;
            }
            return total;
        }

        /// <summary>
        /// Set the vehicle to charge/refuel
        /// </summary>
        public void Charge()
        {
            mStatus = VehicleStatus.Charging;
        }

        /// <summary>
        /// Travel along path while transporting rider
        /// </summary>
        /// <param name="pathNodeIds">List of node ids to visit in sequence, representing a valid path</param>
        public void BeginTrip(List<long> pathNodeIds)
        {
            // If we're already at the location
            if (pathNodeIds.Count == 1)
            {
                TripComplete();
                return;
            }
            var path = mMap.ConstructTravelPath(pathNodeIds);
            mStatus = VehicleStatus.Transporting;
            mTripLog += $"\nTransporting rider ";
            BeginRoute(path);
        }

        /// <summary>
        /// Travel along path (without rider)
        /// </summary>
        /// <param name="pathNodeIds">List of node ids to visit in sequence, representing a valid path</param>
        public void PickUpRider(List<long> pathNodeIds)
        {
            // If we're already at the location
            if (pathNodeIds.Count == 1)
            {
                TripComplete();
                return;
            }
            var path = mMap.ConstructTravelPath(pathNodeIds);
            mStatus = VehicleStatus.PickingUpRider;
            mTripLog += $"\nPicking up rider ";
            BeginRoute(path);
        }

        /// <summary>
        /// Input a valid path that this vehicle will immediately begin to drive.
        /// </summary>
        /// <param name="path">List of edges follow in sequence</param>
        private void BeginRoute(List<GraphEdge> path)
        {
            // TODO: take into account that routes may begin between points, i.e. some distance may be travelled to get to the starting point of the path
            mCurrentPath = path;
            mIndexOnPath = 0;
            mCurrentDestination = path[path.Count - 1].Target.Osmid;
            mLastLocation = path[0].Source.Osmid;
            mDistanceRemainingOnCurrentHop = path[mIndexOnPath].Length;
            mTripLog += $"from {path[0].Source.Y},{path[0].Source.X} to {path[^1].Target.Y},{path[^1].Target.X}";
        }

        /// <summary>
        /// Ask the vehicle if it has enough fuel/charge to complete the route and return to a charging station
        /// </summary>
        /// <param name="path">List of node ids to visit in sequence</param>
        public bool CanDoRoute(List<long> pathNodeIds)
        {
            var path = mMap.ConstructTravelPath(pathNodeIds);
            return CanDoRoute(path);
        }

        /// <summary>
        /// Ask the vehicle if it has enough fuel/charge to complete the route and return to a charging station
        /// </summary>
        /// <param name="path">List edges to follow in sequence</param>
        public bool CanDoRoute(List<GraphEdge> path)
        {
            // TODO: Factor in range for getting to a charging/fuel station
            var length = GetTotalPathLength(path);
            return length < GetRemainingRange();
        }

        public void ResetVehicle()
        {
            mCharge = 100;
            mCurrentDestination = -1;
            mCurrentPath.Clear();
            mDistanceRemainingOnCurrentHop = 0;
            mIndexOnPath = 0;
            mStatus = VehicleStatus.Idle;
            mLastLocation = mMap.GetRandomValidLocation();
            mTripLog = "";
        }

        /// <summary>
        /// Remaining driving range the vehicle believes it has, based on its current charge and fuel/charge efficiency
        /// </summary>
        public double GetRemainingRange()
        {
            // leave a 10% margin of error
            return mMetersPerUnitOfCharge * mCharge * 0.9;
        }

        /// <summary>
        /// Converts k/hr to m/s
        /// </summary>
        /// <returns></returns>
        private static double KphToMps(double speed)
        {
            return speed * 0.2777778;
        }

        /// <summary>
        /// Complete the currently assigned trip
        /// </summary>
        private void TripComplete()
        {
            if (mStatus == VehicleStatus.PickingUpRider)
                mStatus = VehicleStatus.AwaitingRider;
            if (mStatus == VehicleStatus.Transporting)
            {
                mStatus = VehicleStatus.Idle;
                sRidersTransported++;
            }
            mDistanceRemainingOnCurrentHop = 0;
            mTripLog += "\nTrip complete!";
            Console.WriteLine(mTripLog);
            mTripLog = "";
        }

        /// <summary>
        /// Cancel the currently assigned route, clearing the path.
        /// </summary>
        public void CancelCurrentRoute()
        {
            mStatus = VehicleStatus.Idle;
            mCurrentPath.Clear();
        }

        /// <summary>
        /// Simulates vehicle movement for a single time step by recursively moving along path
        /// </summary>
        private void DriveForSeconds(double seconds)
        {
            // Get current speed in m/s
            var speed = KphToMps(mCurrentPath[mIndexOnPath].MaxSpeedKPH);
            var distancePerStep = speed * mSim.TimeStep;

            // If there is less distance remaining on this hop of the path than we travel in a time step,
            // move on to the next hop
            if (mDistanceRemainingOnCurrentHop < distancePerStep)
            {
                // If we're completing the last hop of the path, we have arrived
                if (mIndexOnPath == mCurrentPath.Count - 1)
                {
                    TripComplete();
                    return;
                }

                var hopName = mCurrentPath[mIndexOnPath].Name;
                if (hopName == "")
                    hopName = mCurrentPath[mIndexOnPath].Ref;

                mTripLog += $"\nMoving onto next leg: {hopName}";
                // Move on to the next hop
                mIndexOnPath++;
                mDistanceRemainingOnCurrentHop = mCurrentPath[mIndexOnPath].Length;
                sTotalMetersDriven += mDistanceRemainingOnCurrentHop;
                var fractionOfRemainingTravel = 1 - (mDistanceRemainingOnCurrentHop / distancePerStep);
                DriveForSeconds(seconds * fractionOfRemainingTravel);
            }
            else
            {
                mTripLog += ".";
                sTotalMetersDriven += distancePerStep;
                mDistanceRemainingOnCurrentHop -= distancePerStep;
            }
        }

        public void OnTimeIncrement(object? sender, EventArgs e)
        {
            switch (mStatus)
            {
                case VehicleStatus.Idle:
                    break;

                case VehicleStatus.Charging:
                    var chargeStep = mSim.TimeStep * mSim.ChargeRate;
                    if (mCharge + chargeStep < 100)
                        mCharge += chargeStep;
                    else
                        mStatus = VehicleStatus.Idle;
                    break;

                case VehicleStatus.Transporting:
                    DriveForSeconds(mSim.TimeStep);
                    break;

                case VehicleStatus.PickingUpRider:
                    DriveForSeconds(mSim.TimeStep);
                    break;

                case VehicleStatus.AwaitingRider:
                    break;

                default:
                    throw new NotImplementedException($"Vehicle status not implemented: {mStatus}");
            }
        }
    }
}
