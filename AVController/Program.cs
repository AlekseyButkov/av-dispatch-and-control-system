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
            var dispatcher = new DispatchAgent(nodesAndEdges.Nodes, nodesAndEdges.Edges);
            dispatcher.TestRoutes();
        }
    }
}
