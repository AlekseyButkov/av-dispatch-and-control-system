using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    internal class DispatchAgent
    {
        RouteFinder routeFinder;
        public DispatchAgent(List<GraphNode> nodes, List<GraphMLEdge> edges)
        {
            routeFinder = new RouteFinder(nodes, edges);
        }
    }
}
