using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    /// <summary>
    /// Class representing a GraphML style edge
    /// </summary>
    public record class GraphMLEdge
    {
        public string Service { get; set; } = "";
        public string Junction { get; set; } = "";
        public string Access { get; set; } = "";
        public string Geometry { get; set; } = "";
        public string Name { get; set; } = "";
        public string Highway { get; set; } = "";
        public string Ref { get; set; } = "";
        public string Lanes { get; set; } = "";
        public bool Tunnel { get; set; } = false;
        public bool Bridge { get; set; } = false;
        public bool Oneway { get; set; } = false;
        public double MaxSpeedKPH { get; set; } = double.NaN;
        public double Length { get; set; } = double.NaN;
        public long OsmId { get; set; }
        public long Id { get; set; } = -1;
        public long SourceNodeId = -1;
        public long TargetNodeId = -1;

        public GraphMLEdge(long id, long node1id, long node2id)
        {
            Id = id;
            SourceNodeId = node1id;
            TargetNodeId = node2id;
        }

        public override string ToString()
        {
            return $"Id: {Id}, OsmId{OsmId}, Source: {SourceNodeId}, Target: {TargetNodeId}";
        }
    }
}
