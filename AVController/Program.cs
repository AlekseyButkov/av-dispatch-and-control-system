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
            var sim = new WorldSimulation(5, args[0]);
            //dispatcher.TestRoutes();
            sim.BeginSimulation();
        }
    }
}
