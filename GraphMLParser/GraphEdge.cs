using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMLParser
{
    public class GraphEdge
    {
        public string Service { get; set; } = "";
        public string Junction { get; set; } = "";
        public string Access { get; set; } = "";
        public string Geometry { get; set; } = "";
        public string Name { get; set; } = "";
        public string Highway { get; set; } = "";
        public string Ref { get; set; } = "";
        public string MaxSpeed { get; set; } = "";
        public string Lanes { get; set; } = "";
        public bool Tunnel { get; set; } = false;
        public bool Bridge { get; set; } = false;
        public bool Oneway { get; set; } = false;
        public double Length { get; set; } = double.NaN;
        public long Id { get; set; } = -1;
        public long sourceNodeId = -1;
        public long targetNodeId = -1;

        public GraphEdge(long id, long node1id, long node2id)
        {
            Id = id;
            sourceNodeId = node1id;
            targetNodeId = node2id;
        }
    }
}
