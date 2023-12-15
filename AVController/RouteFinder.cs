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
    internal class RouteFinder
    {
        GraphMap mWorldMap;
        AStarSearch mAStarSearch;
        
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
            Console.WriteLine($"initialized route finder worldmap in {sw.ElapsedMilliseconds}");
        }

        public void FindRoute(GraphNode start, GraphNode end)
        {
            FindRoute(start.Osmid, end.Osmid);
        }

        public void FindRoute(long startId, long endId)
        {
            var path = mAStarSearch.FindShortestPath(startId, endId);
            var start = mWorldMap.GetPosition(startId);
            var end = mWorldMap.GetPosition(endId);
            var distance = 0.0;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var stepDistance = mWorldMap.GetDistance(path[i], path[i + 1]);
                distance += stepDistance;
            }
            Console.WriteLine($"Shortest path from {start.Y},{start.X} to {end.Y},{end.X}:\n{distance}m");
        }

        public void TestRoutingBetweenRandomLocations()
        {
            var start = mWorldMap.RandomlyGenerateDestination();
            var end = mWorldMap.RandomlyGenerateDestination();
            Console.WriteLine($"Trying to route...");
            FindRoute(start, end);
        }
    }
}
