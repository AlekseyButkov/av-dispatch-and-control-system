using AVController;
using AVGraphPrimitives;

namespace Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No GraphML input file supplied");
                return;
            }
            GraphMLParser.GraphMLParser parser = new();
            var map = new GraphMap();
            var nodesAndEdges = parser.ParseFile(args[0]);
            RouteFinder router = new(nodesAndEdges.Nodes, nodesAndEdges.Edges, map);

            while (true)
            {
                router.TestRoutingBetweenRandomLocations();
            }
        }
    }
}
