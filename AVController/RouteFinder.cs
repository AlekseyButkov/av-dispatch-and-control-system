using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Graph;
using AVGraphPrimitives;

namespace AVController
{
    public class RouteFinder
    {
        GraphMap mWorldMap;
        AStarSearch mAStarSearch;
        
        public RouteFinder()
        {
            mWorldMap = new();
            mAStarSearch = new(mWorldMap);
        }

        public RouteFinder(List<GraphNode> nodes, List<GraphMLEdge> edges, GraphMap worldMap)
        {
            mWorldMap = worldMap;
            InitWorldMap(nodes, edges);
            mAStarSearch = new(mWorldMap);
        }

        private void InitWorldMap(List<GraphNode> nodes, List<GraphMLEdge> edges)
        {
            var sw = new Stopwatch();
            sw.Start();
            foreach(var node in nodes)
            {
                mWorldMap.AddLocation(node);
            }
            mWorldMap.Clean();
            foreach(var edge in edges)
            {
                if (edge.Oneway)
                    mWorldMap.AddUnidirectionalLink(edge);
                else
                    mWorldMap.AddBidirectionalLink(edge);
            }
            mWorldMap.RebuildNodeListFromEdges();
            Console.WriteLine($"initialized route finder worldmap in {sw.ElapsedMilliseconds}");
        }

        public void TryFindRoute(GraphNode start, GraphNode end, TypeOfOptimality pathOptimalityMetric)
        {
            TryFindRoute(start.Osmid, end.Osmid, pathOptimalityMetric);
        }

        public List<long> TryFindRoute(long startId, long endId, TypeOfOptimality pathOptimalityMetric)
        {
            if (mWorldMap.IsEmpty)
                throw new Exception("World map is empty");
            var path = mAStarSearch.FindShortestPath(startId, endId, pathOptimalityMetric);
            var start = mWorldMap.GetPosition(startId);
            var end = mWorldMap.GetPosition(endId);
            var distance = 0.0;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var stepDistance = mWorldMap.GetDistanceMetric(path[i], path[i + 1]);
                distance += stepDistance;
            }
            //Console.WriteLine($"Shortest path from {start.Y},{start.X} to {end.Y},{end.X}:\n{distance}m");
            if (path.Count == 0)
            {
                throw new Exception($"Unable to find route. from node {startId} to node {endId}. Positions: {start.Y},{start.X} to {end.Y},{end.X}");
            }
            return path;
        }

        public void TestRoutingBetweenRandomLocations()
        {
            var start = mWorldMap.RandomlyGenerateDestination();
            var end = mWorldMap.RandomlyGenerateDestination();
            Console.WriteLine($"Trying to route...");
            TryFindRoute(start, end, TypeOfOptimality.Time);
        }
    }
}
