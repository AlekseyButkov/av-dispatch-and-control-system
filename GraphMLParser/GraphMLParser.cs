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
        Dictionary<string, string> mEdgeAttrMap = new();
        Dictionary<string, string> mNodeAttrMap = new();

        public (List<GraphNode> Nodes, List<GraphMLEdge> Edges) ParseFile(string file)
        {
            mEdgeAttrMap.Clear();
            mNodeAttrMap.Clear();
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

            var keys = doc.Root.Elements(ns + "key").ToList();
            foreach (var key in keys)
            {
                if (key.Attribute("for")!.Value == "edge")
                    mEdgeAttrMap[key.Attribute("attr.name")!.Value] = key.Attribute("id")!.Value;
                if (key.Attribute("for")!.Value == "node")
                    mNodeAttrMap[key.Attribute("attr.name")!.Value] = key.Attribute("id")!.Value;
            }

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

            var osmid = element.Attribute("id") == null ? 0 : long.Parse(element.Attribute("id")!.Value);
            var edge = new GraphMLEdge(mEdgeIdHelper, sourceNode, targetNode);

            // speed, length, and oneway must be present, otherwise the dataset contains improperly formed edges
            var maxspeed = GetEdgeElement("maxspeed");
            edge.MaxSpeedKPH = maxspeed.Count() == 0 ? DEFAULT_SPEED : ParseSpeedToKph(maxspeed.First().Value);
            var length = GetEdgeElement("length");
            edge.Length = length.Count() == 0 ? double.PositiveInfinity : double.Parse(length.First().Value);
            var oneway = GetEdgeElement("oneway");
            edge.Oneway = oneway.Count() == 0 ? false : bool.Parse(oneway.First().Value);

            if (mEdgeAttrMap.ContainsKey("service"))
            {
                var service = GetEdgeElement("service");
                edge.Service = service.Count() == 0 ? "" : service.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("junction"))
            {
                var junction = GetEdgeElement("junction");
                edge.Junction = junction.Count() == 0 ? "" : junction.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("tunnel"))
            {
                var tunnel = GetEdgeElement("tunnel");
                edge.Tunnel = tunnel.Count() == 0 ? false : tunnel.First().Value == "yes";
            }
            if (mEdgeAttrMap.ContainsKey("access"))
            {
                var access = GetEdgeElement("access");
                edge.Access = access.Count() == 0 ? "" : access.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("bridge"))
            {
                var bridge = GetEdgeElement("bridge");
                edge.Bridge = bridge.Count() == 0 ? false : bridge.First().Value == "yes";
            }
            if (mEdgeAttrMap.ContainsKey("lanes"))
            {
                var lanes = GetEdgeElement("lanes");
                edge.Lanes = lanes.Count() == 0 ? "" : lanes.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("geometry"))
            {
                var geometry = GetEdgeElement("geometry");
                edge.Geometry = geometry.Count() == 0 ? "" : geometry.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("highway"))
            {
                var highway = GetEdgeElement("highway");
                edge.Highway = highway.Count() == 0 ? "" : highway.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("name"))
            {
                var name = GetEdgeElement("name");
                edge.Name = name.Count() == 0 ? "" : name.First().Value;
            }
            if (mEdgeAttrMap.ContainsKey("ref"))
            {
                var reference = GetEdgeElement("ref");
                edge.Ref = reference.Count() == 0 ? "" : reference.First().Value;
            }
            mEdgeIdHelper++;

            return edge;

            IEnumerable<XElement> GetEdgeElement(string name)
            {
                var blah = data.Where(x => x.Attribute("key")!.Value == mEdgeAttrMap[name]);
                return blah;
            }
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
            var x = double.Parse(data.Where(x => x.Attribute("key")!.Value == mNodeAttrMap["x"]).First().Value);
            var y = double.Parse(data.Where(x => x.Attribute("key")!.Value == mNodeAttrMap["y"]).First().Value);
            var osmid = long.Parse(element.Attribute("id")!.Value);
            var node = new GraphNode(mNodeIdHelper, osmid, x, y);

            mNodeIdHelper++;
            return node;
        }
    }
}
