using aima.core.environment.map;
using aima.core.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public class AStarSearch
    {
        private readonly Map mMap;
        private Dictionary<SourceDest, double> mEstimateCache = new();

        public AStarSearch(Map map)
        {
            mMap = map ?? throw new ArgumentNullException(nameof(map));
        }

        public List<string> FindShortestPath(string startLocation, string goalLocation)
        {
            if (!mMap.getLocations().Contains(startLocation) || !mMap.getLocations().Contains(goalLocation))
            {
                throw new ArgumentException("Invalid start or goal location.");
            }

            var openSet = new List<string> { startLocation };
            var cameFrom = new Dictionary<string, string>();
            var gScore = new Dictionary<string, double> { { startLocation, 0 } };
            var fScore = new Dictionary<string, double> { { startLocation, HeuristicEstimate(startLocation, goalLocation) } };

            while (openSet.Any())
            {
                var current = openSet.OrderBy(loc => fScore[loc]).First();

                if (current == goalLocation)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (var neighbor in mMap.getPossibleNextLocations(current))
                {
                    var tentativeGScore = gScore[current] + GetDistance(current, neighbor);

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

            return new List<string>(); // No path found
        }

        private double HeuristicEstimate(string fromLocation, string toLocation)
        {
            // Example heuristic: Euclidean distance between two locations
            Point2D fromPosition = mMap.getPosition(fromLocation);
            Point2D toPosition = mMap.getPosition(toLocation);
            var trip = new SourceDest(fromLocation, toLocation);
            double estimate;
            if (mEstimateCache.ContainsKey(trip))
                estimate = mEstimateCache[trip];
            else
            {
                estimate = Math.Sqrt(Math.Pow(fromPosition.getX() - toPosition.getX(), 2) + Math.Pow(fromPosition.getY() - toPosition.getY(), 2));
                mEstimateCache[trip] = estimate;
            }
            return estimate;
        }

        private double GetDistance(string fromLocation, string toLocation)
        {
            double? distance = mMap.getDistance(fromLocation, toLocation);
            return distance ?? double.PositiveInfinity;
        }

        private List<string> ReconstructPath(Dictionary<string, string> cameFrom, string current)
        {
            var path = new List<string> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }
}
