using AVGraphPrimitives;

namespace AVController
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
            var nodesAndEdges = parser.ParseFile(args[0]);
            var map = new GraphMap();
            var routeFinder = new RouteFinder(nodesAndEdges.Nodes, nodesAndEdges.Edges, map);
            var dispatcher = new DispatchAgent(routeFinder, map);
            var sim = new WorldSimulation(map, dispatcher, 5);
            //dispatcher.TestRoutes();
            sim.BeginSimulation();
        }
    }
}
