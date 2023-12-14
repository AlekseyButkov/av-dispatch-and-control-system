using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGraphPrimitives
{
    public record class GraphEdge : GraphMLEdge
    {
        private GraphNode source;
        private GraphNode target;
        public GraphEdge(long id, long node1id, long node2id, GraphNode node1, GraphNode node2) : base(id, node1id, node2id)
        {
            source = node1;
            target = node2;
        }
        public GraphEdge(GraphMLEdge edge, GraphNode node1, GraphNode node2) : base(edge)
        {
            source = node1;
            target = node2;
        }

        public GraphNode Source
        { get { return source; } }
        public GraphNode Target
        { get { return target; } }
    }
}
