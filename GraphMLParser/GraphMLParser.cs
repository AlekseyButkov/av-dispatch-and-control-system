using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GraphMLParser
{
    public class GraphMLParser
    {
        public static (List<GraphNode>, List<GraphEdge>) ParseFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Could not find {file}");

            List<GraphNode> nodes = new();
            List<GraphEdge> edges = new();

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

        static GraphEdge GetGarphEdge(XElement? element, XNamespace ns)
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

            var id = element.Attribute("id") == null ? 0 : long.Parse(element.Attribute("id")!.Value);
            var edge = new GraphEdge(id, sourceNode, targetNode);
            edge.Service = service.Count() == 0 ? "" : service.First().Value;
            edge.Junction = junction.Count() == 0 ? "" : junction.First().Value;
            edge.Tunnel = tunnel.Count() == 0 ? false : tunnel.First().Value == "yes";
            edge.Access = access.Count() == 0 ? "" : access.First().Value;
            edge.Bridge = bridge.Count() == 0 ? false : bridge.First().Value == "yes";
            edge.Lanes = lanes.Count() == 0 ? "" : lanes.First().Value;
            edge.Geometry = geometry.Count() == 0 ? "" : geometry.First().Value;
            edge.Length = length.Count() == 0 ? double.PositiveInfinity : double.Parse(length.First().Value);
            edge.MaxSpeed = maxspeed.Count() == 0 ? "" : maxspeed.First().Value;
            edge.Highway = highway.Count() == 0 ? "" : highway.First().Value;
            edge.Name = name.Count() == 0 ? "" : name.First().Value;
            edge.Ref = reference.Count() == 0 ? "" : reference.First().Value;
            edge.Oneway = oneway.Count() == 0 ? false : bool.Parse(oneway.First().Value);

            return edge;
        }

        static GraphNode GetGraphNode(XElement? element, XNamespace ns)
        {
            if (element == null) throw new ArgumentNullException();
            var data = element.Elements(ns + "data");
            var x = double.Parse(data.Where(x => x.Attribute("key")!.Value == "d5").First().Value);
            var y = double.Parse(data.Where(x => x.Attribute("key")!.Value == "d4").First().Value);
            var id = long.Parse(element.Attribute("id")!.Value);
            var node = new GraphNode(id, x, y);

            return node;
        }
    }
}
