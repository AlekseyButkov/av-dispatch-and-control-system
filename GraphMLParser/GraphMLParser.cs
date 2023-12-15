using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Graph;

namespace GraphMLParser
{
    public class GraphMLParser
    {
        private const double DEFAULT_SPEED = 50; // kph
        private const double MPH_TO_KPH_FACTOR = 1.609344;
        private long mNodeIdHelper;
        private long mEdgeIdHelper;
        private Regex speedRx = new(@"\d+");
        public (List<GraphNode> Nodes, List<GraphMLEdge> Edges) ParseFile(string file)
        {
            mNodeIdHelper = 0;
            mEdgeIdHelper = 0;
            if (!File.Exists(file))
                throw new FileNotFoundException($"Could not find {file}");

            List<GraphNode> nodes = new();
            List<GraphMLEdge> edges = new();

            XDocument doc = XDocument.Load(file);
            if (doc == null)
                throw new Exception("doc is null");
            if (doc.Root == null)
                throw new Exception("root is null");

            var xmlnsVal = doc.Root.Attribute("xmlns");
            if (xmlnsVal == null)
                throw new Exception("no namespace");
            XNamespace ns = xmlnsVal.Value;

            var docEdges = (from i in doc.Root.Element(ns + "graph")!.Elements(ns + "edge")
                           select i).ToList();
            var docNodes = (from i in doc.Root.Element(ns + "graph")!.Elements(ns + "node")
                            select i).ToList();

            foreach (var edgeElement in docEdges)
            {
                var edge = GetGarphEdge(edgeElement, ns);
                edges.Add(edge);
            }
            foreach (var nodeElement in docNodes)
            {
                var node = GetGraphNode(nodeElement, ns);
                nodes.Add(node);
            }

            if (nodes == null) throw new Exception("node list is edge");
            if (edges == null) throw new Exception("node list is null");

            return (nodes, edges);
        }

        GraphMLEdge GetGarphEdge(XElement? element, XNamespace ns)
        {
            if (element == null) throw new ArgumentNullException();
            var data = element.Elements(ns + "data");
            var sourceNode = long.Parse(element.Attribute("source")!.Value);
            var targetNode = long.Parse(element.Attribute("target")!.Value);
            var service = data.Where(x => x.Attribute("key")!.Value == "d22");
            var junction = data.Where(x => x.Attribute("key")!.Value == "d21");
            var tunnel = data.Where(x => x.Attribute("key")!.Value == "d20");
            var access = data.Where(x => x.Attribute("key")!.Value == "d19");
            var bridge = data.Where(x => x.Attribute("key")!.Value == "d18");
            var lanes = data.Where(x => x.Attribute("key")!.Value == "d17");
            var geometry = data.Where(x => x.Attribute("key")!.Value == "d16");
            var length = data.Where(x => x.Attribute("key")!.Value == "d15");
            var maxspeed = data.Where(x => x.Attribute("key")!.Value == "d14");
            var highway = data.Where(x => x.Attribute("key")!.Value == "d13");
            var name = data.Where(x => x.Attribute("key")!.Value == "d12");
            var reference = data.Where(x => x.Attribute("key")!.Value == "d11");
            var oneway = data.Where(x => x.Attribute("key")!.Value == "d10");

            var osmid = element.Attribute("id") == null ? 0 : long.Parse(element.Attribute("id")!.Value);
            var edge = new GraphMLEdge(mEdgeIdHelper, sourceNode, targetNode);
            edge.Service = service.Count() == 0 ? "" : service.First().Value;
            edge.Junction = junction.Count() == 0 ? "" : junction.First().Value;
            edge.Tunnel = tunnel.Count() == 0 ? false : tunnel.First().Value == "yes";
            edge.Access = access.Count() == 0 ? "" : access.First().Value;
            edge.Bridge = bridge.Count() == 0 ? false : bridge.First().Value == "yes";
            edge.Lanes = lanes.Count() == 0 ? "" : lanes.First().Value;
            edge.Geometry = geometry.Count() == 0 ? "" : geometry.First().Value;
            edge.Length = length.Count() == 0 ? double.PositiveInfinity : double.Parse(length.First().Value);
            edge.MaxSpeedKPH = maxspeed.Count() == 0 ? DEFAULT_SPEED : ParseSpeedToKph(maxspeed.First().Value);
            edge.Highway = highway.Count() == 0 ? "" : highway.First().Value;
            edge.Name = name.Count() == 0 ? "" : name.First().Value;
            edge.Ref = reference.Count() == 0 ? "" : reference.First().Value;
            edge.Oneway = oneway.Count() == 0 ? false : bool.Parse(oneway.First().Value);

            mEdgeIdHelper++;

            return edge;
        }

        /// <summary>
        /// Parses speed string and converts from MPH to KPH
        /// </summary>
        /// <param name="speed"></param>
        double ParseSpeedToKph(string speed)
        {
            var matches = speedRx.Matches(speed);
            if (matches.Count == 0)
                return DEFAULT_SPEED;
            else
                return double.Parse(matches[0].Value) * MPH_TO_KPH_FACTOR;
        }

        GraphNode GetGraphNode(XElement? element, XNamespace ns)
        {
            if (element == null) throw new ArgumentNullException();
            var data = element.Elements(ns + "data");
            var x = double.Parse(data.Where(x => x.Attribute("key")!.Value == "d5").First().Value);
            var y = double.Parse(data.Where(x => x.Attribute("key")!.Value == "d4").First().Value);
            var osmid = long.Parse(element.Attribute("id")!.Value);
            var node = new GraphNode(mNodeIdHelper, osmid, x, y);

            mNodeIdHelper++;
            return node;
        }
    }
}
