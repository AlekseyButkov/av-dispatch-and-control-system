using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGraphPrimitives
{
    public class GraphMap : IMap
    {
        private const double KPH_TO_MPS_FACTOR = 0.2777778;
        private Dictionary<long, GraphNode> mLocations = new();
        private Dictionary<(long, long), GraphEdge> mEdges = new();
        private List<long> mValidLocations = new();
        private DirectedGraph mGraph = new();
        private string mSourceFileName = string.Empty;
        public GraphMap() { }

        public bool IsEmpty { get { return mEdges.Count == 0; } }
        public string SourceFileName
        {
            get { return mSourceFileName; }
            set { mSourceFileName = value; }
        }
        public List<long> ValidLocations
        {
            get { return mValidLocations; }
            set { mValidLocations = value; }
        }

        public void AddLocation(GraphNode location)
        {
            if (!mLocations.ContainsKey(location.Osmid))
            {
                mLocations.Add(location.Osmid, location);
            }
            else
                Console.WriteLine($"Tried to add already existing location: {location}");
        }

        /// <summary>
        /// Gets the 2 nodes connected by the provided edge, as identified by their ID
        /// </summary>
        /// <returns>A tuple containing the source and destination nodes in the edge</returns>
        /// <exception cref="Exception"></exception>
        private (GraphNode Source, GraphNode Destination) GetSourceAndDest(GraphMLEdge edge)
        {
            if (!mLocations.ContainsKey(edge.SourceNodeId))
                throw new Exception($"Tried to add bidirectional link from nonexistent node: {edge.SourceNodeId}");
            if (!mLocations.ContainsKey(edge.SourceNodeId))
                throw new Exception($"Tried to add bidirectional link to nonexistent node: {edge.TargetNodeId}");
            var source = mLocations[edge.SourceNodeId];
            var dest = mLocations[edge.TargetNodeId];
            return (source, dest);
        }

        /// <summary>
        /// Removes any nodes which have no edges connected to them.
        /// </summary>
        public void Clean()
        {
            var disconnectedNodes = mGraph.GetIsolatedNodes();
            foreach (var g in disconnectedNodes)
                mLocations.Remove(g.Osmid);
        }

        /// <summary>
        /// Rebuilds the node list by only using nodes which edges in the underlying graph connect.
        /// </summary>
        public void RebuildNodeListFromEdges()
        {
            var nodes = mGraph.GetNodesWithConnections();
            mLocations.Clear();
            foreach (var n in nodes)
                mLocations[n.Osmid] = n;
        }

        public GraphEdge? GetEdge(long node1, long node2)
        {
            if (!mEdges.ContainsKey((node1, node2)))
                return null;
            return mEdges[(node1, node2)];
        }

        public List<GraphEdge> ConstructTravelPath(List<long> ids)
        {
            List<GraphEdge> path = new();
            for (int i = 0; i < ids.Count - 1; i++)
            {
                var hop = GetEdge(ids[i], ids[i + 1]);
                if (hop == null)
                    throw new Exception($"Cannot construct a path, edge from {ids[i]} to {ids[i + 1]} was not found!");
                path.Add(hop);
            }
            return path;
        }

        public void AddBidirectionalLink(GraphMLEdge edge)
        {
            var endPoints = GetSourceAndDest(edge);
            var directedEdge1 = new GraphEdge(edge, endPoints.Source, endPoints.Destination);
            var directedEdge2 = new GraphEdge(edge, endPoints.Destination, endPoints.Source);
            mGraph.AddDirectedEdge(directedEdge1);
            mGraph.AddDirectedEdge(directedEdge2);
            mEdges[(directedEdge1.Source.Osmid, directedEdge1.Target.Osmid)] = directedEdge1;
            mEdges[(directedEdge2.Source.Osmid, directedEdge2.Target.Osmid)] = directedEdge2;
        }

        public void AddUnidirectionalLink(GraphMLEdge edge)
        {
            var endPoints = GetSourceAndDest(edge);
            var directedEdge = new GraphEdge(edge, endPoints.Source, endPoints.Destination);
            mGraph.AddDirectedEdge(directedEdge);
            mEdges[(directedEdge.Source.Osmid, directedEdge.Target.Osmid)] = directedEdge;
        }

        public List<long> GetLocations()
        {
            return mLocations.Keys.ToList();
        }

        public List<GraphNode> GetLocationNodes()
        {
            return mLocations.Values.ToList();
        }

        public List<long> GetPossibleNextLocations(long location)
        {
            var node = mGraph.GetNode(location);
            var nextLocations = new List<long>();
            for (var i = 0; i < node.OutboundEdges.Count; i++)
            {
                nextLocations.Add(node.OutboundEdges[i].Target.Osmid);
            }
            return nextLocations;
        }

        public List<long> GetPossiblePrevLocations(long location)
        {
            var node = mGraph.GetNode(location);
            var prevLocations = new List<long>();
            for (var i = 0; i < node.InboundEdges.Count; i++)
            {
                prevLocations.Add(node.InboundEdges[i].Target.Osmid);
            }
            return prevLocations;
        }

        public double GetDistanceMetric(long fromLocation, long toLocation)
        {
            var fromNode = mGraph.GetNode(fromLocation);
            var toNode = mGraph.GetNode(toLocation);
            var connectionList = mGraph.GetConnections(fromNode);
            for (var i = 0; i < connectionList.Count; i++)
            {
                if (connectionList[i].Node == toNode)
                    return connectionList[i].Distance;
            }
            return double.PositiveInfinity;
        }

        public double GetTimeMetric(long fromLocation, long toLocation)
        {
            var fromNode = mGraph.GetNode(fromLocation);
            var toNode = mGraph.GetNode(toLocation);
            var connectionList = mGraph.GetConnections(fromNode);
            for (var i = 0; i < connectionList.Count; i++)
            {
                if (connectionList[i].Node == toNode)
                {
                    var seconds = connectionList[i].Distance / (connectionList[i].Edge.MaxSpeedKPH * KPH_TO_MPS_FACTOR);
                    return seconds;
                }
            }
            return double.PositiveInfinity;
        }

        public Point2D GetPosition(long loc)
        {
            var node = mGraph.GetNode(loc);
            return new Point2D(node.X, node.Y);
        }

        /// <summary>
        /// Get a random location. Location may be part of a graph that is not connected to the main graph.
        /// </summary>
        /// <returns>Id of the location</returns>
        public long GetRandomLocation()
        {
            var rand = new Random();
            return mLocations.ElementAt(rand.Next(mLocations.Count)).Key;
        }

        /// <summary>
        /// Gets a random location from a list of previously verified valid locations
        /// </summary>
        /// <returns></returns>
        public long GetRandomValidLocation()
        {
            var rand = new Random();
            return mValidLocations.ElementAt(rand.Next(mValidLocations.Count));
        }

        /// <summary>
        /// Remove a location from the available locations dictionary.
        /// </summary>
        public void RemoveLocation(long nodeId)
        {
            mLocations.Remove(nodeId);
        }
    }
}
