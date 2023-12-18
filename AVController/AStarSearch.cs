using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AVGraphPrimitives;

namespace AVController
{
    public enum TypeOfOptimality
    {
        Distance,
        Time
    }

    public class AStarSearch
    {
        private readonly IMap mMap;
        private Dictionary<SourceDest, double> mEstimateCache = new();

        public AStarSearch(IMap map)
        {
            mMap = map ?? throw new ArgumentNullException(nameof(map));
        }

        public List<long> FindShortestPath(long startLocation, long goalLocation, TypeOfOptimality lengthType)
        {
            if (!mMap.GetLocations().Contains(startLocation) || !mMap.GetLocations().Contains(goalLocation))
            {
                throw new ArgumentException("Invalid start or goal location.");
            }

            var openSet = new List<long> { startLocation };
            var cameFrom = new Dictionary<long, long>();
            var gScore = new Dictionary<long, double> { { startLocation, 0 } };
            var fScore = new Dictionary<long, double> { { startLocation, HeuristicEstimate(startLocation, goalLocation) } };

            while (openSet.Any())
            {
                var current = openSet.OrderBy(loc => fScore[loc]).First();

                if (current == goalLocation)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (var neighbor in mMap.GetPossibleNextLocations(current))
                {
                    var tentativeGScore = gScore[current] + GetEdgeLength(current, neighbor, lengthType);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + HeuristicEstimate(neighbor, goalLocation);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return new List<long>(); // No path found
        }

        private double HeuristicEstimate(long fromLocation, long toLocation)
        {
            // Example heuristic: Euclidean distance between two locations
            Point2D fromPosition = mMap.GetPosition(fromLocation);
            Point2D toPosition = mMap.GetPosition(toLocation);
            var trip = new SourceDest(fromLocation, toLocation);
            double estimate;
            if (mEstimateCache.ContainsKey(trip))
                estimate = mEstimateCache[trip];
            else
            {
                estimate = Math.Sqrt(Math.Pow(fromPosition.X - toPosition.X, 2) + Math.Pow(fromPosition.Y - toPosition.Y, 2));
                mEstimateCache[trip] = estimate;
            }
            return estimate;
        }

        private double GetEdgeLength(long fromLocation, long toLocation, TypeOfOptimality lengthType)
        {
            var distance = double.PositiveInfinity;
            switch (lengthType)
            {
                case TypeOfOptimality.Distance:
                    distance = mMap.GetDistanceMetric(fromLocation, toLocation);
                    break;
                case TypeOfOptimality.Time:
                    distance = mMap.GetTimeMetric(fromLocation, toLocation);
                    break;
                default: throw new NotImplementedException($"Edge length type {lengthType} not implemented");

            }
            return distance;
        }

        private List<long> ReconstructPath(Dictionary<long, long> cameFrom, long current)
        {
            var path = new List<long> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }
}
