using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Graph;
using aima.core.environment.map;

namespace AVController
{
    internal class RouteFinder
    {
        ExtendableMap mWorldMap = new();
        
        public RouteFinder(List<GraphNode> nodes, List<GraphEdge> edges)
        {
            InitWorldMap(nodes, edges);
        }

        private void InitWorldMap(List<GraphNode> nodes, List<GraphEdge> edges)
        {
            var sw = new Stopwatch();
            sw.Start();
            foreach(var edge in edges)
            {
                if (edge.Oneway)
                    mWorldMap.addUnidirectionalLink(edge.sourceNodeId.ToString(), edge.targetNodeId.ToString(), edge.Length);
                else
                    mWorldMap.addBidirectionalLink(edge.sourceNodeId.ToString(), edge.targetNodeId.ToString(), edge.Length);
            }
            Console.WriteLine($"initialized route finder worldmap in {sw.ElapsedMilliseconds}");
        }

        public void FindRoute(GraphNode start, GraphNode end)
        {
            FindRoute(start.Osmid, end.Osmid);
        }

        public void FindRoute(long startId, long endId)
        {

        }

    }
}
