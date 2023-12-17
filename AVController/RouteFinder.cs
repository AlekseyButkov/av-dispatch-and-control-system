using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Graph;
using AVGraphPrimitives;

namespace AVController
{
    public class RouteFinder
    {
        const int MAX_ROUTING_ATTEMPTS = 10;
        GraphMap mWorldMap;
        AStarSearch mAStarSearch;
        List<long> test = new() { 1, 2, 3, 4, 5, 6};
        
        public RouteFinder()
        {
            mWorldMap = new();
            mAStarSearch = new(mWorldMap);
        }

        public RouteFinder(List<GraphNode> nodes, List<GraphMLEdge> edges, GraphMap worldMap)
        {
            mWorldMap = worldMap;
            mAStarSearch = new(mWorldMap);
            InitWorldMap(nodes, edges);
        }

        private void SerializeNodesIds(List<long> nodes, string filename)
        {
            var json = JsonSerializer.Serialize(nodes);
            File.WriteAllText(filename, json);
        }

        private List<long>? DeserializeNodeIds(string filename)
        {
            var json = File.ReadAllText(filename);
            var list = JsonSerializer.Deserialize<List<long>>(json);
            return list;
        }

        /// <summary>
        /// Gets the locations of the main connected graph. Datasets have disjoint graphs, we ony care about the main graph.
        /// Take each location and attempt to route from it to a number of random other nodes. If we consistently fail, to
        /// route, this location is almost certainly part of a small disconnected graph, and should be ignored.
        /// This is basically an ugly hack to compensate for the fact that datasets were assumed to be a single connected graph.
        /// Results are saved to file and retrieved if they already exist so we don't have to do this every time.
        /// </summary>
        /// <returns></returns>
        private List<long> FindConnectedGraphLocations()
        {

            var validNodes = new List<long>();
            var nodeFile = mWorldMap.SourceFileName + ".json";
            if (File.Exists(nodeFile))
            {
                validNodes = DeserializeNodeIds(nodeFile);
                if (validNodes == null)
                    throw new Exception("Failed to deserialize saved nodes");
            }
            else
            {
                var nodes = mWorldMap.GetLocationNodes();
                for (int i = 0; i < nodes.Count; i++)
                {
                    var valid = IsPartOfConnectedGraph(nodes[i].Osmid);
                    if (valid)
                        validNodes.Add(nodes[i].Osmid);
                    Console.WriteLine($"Tested node {i}: {valid}");
                }
                SerializeNodesIds(validNodes, nodeFile);
            }
            return validNodes;
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
            var validLocIds = FindConnectedGraphLocations();
            mWorldMap.ValidLocations = validLocIds;
            Console.WriteLine($"initialized route finder worldmap in {sw.ElapsedMilliseconds}");
        }

        public void FindRoute(GraphNode start, GraphNode end, TypeOfOptimality pathOptimalityMetric)
        {
            FindRoute(start.Osmid, end.Osmid, pathOptimalityMetric);
        }

        public List<long> FindRoute(long startId, long endId, TypeOfOptimality pathOptimalityMetric)
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
                Console.WriteLine($"Unable to find route. from node {startId} to node {endId}. Positions: {start.Y},{start.X} to {end.Y},{end.X}");
            return path;
        }

        /// <summary>
        /// Tests the node by routing to random locations from it. High degree of failure indicates that the node is part of a disjoint subgraph.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns>True if node is part of main graph, false if it is likely part of a disjoint subgraph</returns>
        public bool IsPartOfConnectedGraph(long nodeId)
        {
            var failed = 0;
            for (var attempts = 0; attempts < MAX_ROUTING_ATTEMPTS; attempts++)
            {
                var dest = mWorldMap.GetRandomLocation();
                var path = FindRoute(nodeId, dest, TypeOfOptimality.Distance);
                if (path.Count == 0)
                    failed++;
            }
            return failed < MAX_ROUTING_ATTEMPTS / 2;
        }

        public void TestRoutingBetweenRandomLocations()
        {
            var start = mWorldMap.GetRandomLocation();
            var end = mWorldMap.GetRandomLocation();
            Console.WriteLine($"Trying to route...");
            FindRoute(start, end, TypeOfOptimality.Time);
        }
    }
}
