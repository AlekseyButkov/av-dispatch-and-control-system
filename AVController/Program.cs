using AVGraphPrimitives;

namespace AVController
{
    internal class Program
    {
        const string useage = "USEAGE:\n" +
            "[Agent Type Selection] [Hours To Simulate] [path to graphml file] [optional: number of starting cars]\n" +
            "[Agent Type Selection]:\n" +
            "       0: no agent - each driver has their own car and drives themselves\n" +
            "       1: default dispatch agent - dispatch agent uses the minimum number of cars possible to satisfy current requests,\n" +
            "                                   always routes nearest available vehicle to pick up riders.\n\n" +
            "[Hours To Simulate]: number of hours that will pass in the simulation before it terminates\n\n" +
            "[Path to graphml file]: the path to the graphml file which is used to build the simulated world and do routing\n\n" +
            "[optional: number of starting cars]: For agents, the number of cars the fleet starts with\n\n\n" +
            "Problem encountered:";
        static void Main(string[] args)
        {
            Type agentType = typeof(IndependentDriverAgent);
            try
            {

                switch (int.Parse(args[0]))
                {
                    case 1:
                        agentType = typeof(DispatchAgent);
                        break;
                    default:
                        
                        agentType = typeof(IndependentDriverAgent);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(useage + ex.Message);
            }
            var numCars = 0;
            if (args.Length >= 4)
            {
                numCars = int.Parse(args[3]);
            }
            var hoursToSimulate = double.Parse(args[1]);
            Console.WriteLine($"Using agent: {agentType.Name}");
            var sim = new WorldSimulation(agentType, 5, args[2], numCars);
            sim.SimulateHours(hoursToSimulate);
            //dispatcher.TestRoutes();
        }
    }
}
