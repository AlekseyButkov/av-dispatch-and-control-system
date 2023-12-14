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
        GraphMap mWorldMap = new();
        
        public RouteFinder(List<GraphNode> nodes, List<GraphMLEdge> edges)
        {
            InitWorldMap(nodes, edges);
        }

        private void InitWorldMap(List<GraphNode> nodes, List<GraphMLEdge> edges)
        {
            var sw = new Stopwatch();
            sw.Start();
            foreach(var node in nodes)
            {
                mWorldMap.AddLocation(node);
            }
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

        }

    }
}
