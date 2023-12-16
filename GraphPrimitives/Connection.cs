using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGraphPrimitives
{
    public struct Connection
    {
        public Connection(GraphNode node, GraphMLEdge edge)
        {
            Node = node;
            Edge = edge;
            Distance = edge.Length;
        }
        public double Distance;
        public GraphNode Node;
        public GraphMLEdge Edge;
    }
}
