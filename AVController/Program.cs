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
            Type agentType = typeof(IndependentDriverAgent);
            switch (int.Parse(args[0]))
            {
                case 1:
                    agentType = typeof(DispatchAgent);
                    break;
                default:
                    agentType = typeof(IndependentDriverAgent);
                    break;
            }
            Console.WriteLine($"Using agent: {agentType.Name}");
            var sim = new WorldSimulation(agentType, 5, args[2]);
            var hoursToSimulate = double.Parse(args[1]);
            sim.SimulateHours(hoursToSimulate);
            //dispatcher.TestRoutes();
        }
    }
}
