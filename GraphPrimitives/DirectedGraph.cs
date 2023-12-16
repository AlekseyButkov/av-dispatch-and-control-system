using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGraphPrimitives
{
    public class DirectedGraph
    {
        private Dictionary<GraphNode, List<Connection>> mConnectionMap = new();
        private Dictionary<long, GraphNode> mNodesMap = new();
        public DirectedGraph() { }

        /// <summary>
        /// Adds a directed edge between two existing or new nodes if one does not already exist.
        /// </summary>
        /// <param name="edge">Edge to add</param>
        /// <param name="source">Source node</param>
        /// <param name="dest">Destination node</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddDirectedEdge(GraphEdge edge)
        {
            if (edge.Source == null)
                throw new ArgumentException($"Source node null when adding directed connection. Edge:{edge}");
            if (edge.Target == null)
                throw new ArgumentException($"Target node null when adding directed connection. Edge:{edge}");

            // add new nodes as they appear
            if (!mConnectionMap.ContainsKey(edge.Source))
            {
                mConnectionMap.Add(edge.Source, new List<Connection>());
                
                AddNode(edge.Target);
            }
            if (!mNodesMap.ContainsKey(edge.Source.Osmid))
                AddNode(edge.Source);
            if (!mNodesMap.ContainsKey(edge.Target.Osmid))
                AddNode(edge.Target);

            // add a new connection to the node
            var connection = new Connection(edge.Target, edge);
            if (!mConnectionMap[edge.Source].Contains(connection))
            {
                if (!edge.Source.OutboundEdges.Contains(edge))
                    edge.Source.OutboundEdges.Add(edge);
                if (!edge.Target.InboundEdges.Contains(edge))
                    edge.Target.InboundEdges.Add(edge);
                mConnectionMap[edge.Source].Add(connection);
            }
        }

        /// <summary>
        /// Removes an edge. Edge source and destination nodes should not be null
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(GraphEdge edge)
        {
            if (edge.Source == null)
            {
                Console.WriteLine($"Failed to remove edge: {edge}, Source was null");
                return;
            }
            if (edge.Target == null)
            {
                Console.WriteLine($"Failed to remove edge: {edge}, Destination was null");
                return;
            }
            var connection = new Connection(edge.Source, edge);
            mConnectionMap[edge.Source].Remove(connection);
        }

        /// <summary>
        /// Adds a node to the list of nodes.
        /// </summary>
        /// <param name="node">Node to add</param>
        private void AddNode(GraphNode node)
        {
            mNodesMap[node.Osmid] = node;
        }

        /// <summary>
        /// Gets all nodes which are not connected to anything
        /// </summary>
        /// <returns></returns>
        public List<GraphNode> GetUnconnectedNodes()
        {
            var nodes = new List<GraphNode>();
            foreach (GraphNode node in mNodesMap.Values)
            {
                if (mConnectionMap[node].Count == 0)
                    nodes.Add(node);
            }
            return nodes;
        }

        public List<GraphNode> GetNodesWithConnections()
        {
            var nodes = mConnectionMap.Where(x => x.Value.Count != 0).Select(x => x.Key).ToList();
            return nodes;
        }

        /// <summary>
        /// Clear all nodes and edges
        /// </summary>c
        public void Clear()
        {
            mConnectionMap.Clear();
            mNodesMap.Clear();
        }

        public GraphNode GetNode(long nodeId)
        {
            return mNodesMap[nodeId];
        }

        public List<Connection> GetConnections(GraphNode node)
        {
            return mConnectionMap[node];
        }
    }
}
