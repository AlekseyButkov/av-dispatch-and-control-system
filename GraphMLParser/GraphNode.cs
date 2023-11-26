using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMLParser
{
    public class GraphNode
    {
        // Member variables for nodes
        public string RefNode { get; set; }
        public string HighwayNode { get; set; }
        public long Osmid { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public GraphNode(long id, double x, double y, string highwayNode = "", string refNode = "")
        {
            Osmid = id;
            X = x;
            Y = y;
            HighwayNode = highwayNode;
            RefNode = refNode;
        }
    }
}
