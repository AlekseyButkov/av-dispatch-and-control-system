using aima.core.environment.map;
using AVGraphPrimitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public class WorldSimulation
    {
        private const int DEFAULT_POPULATION = 430000;
        private List<Vehicle> mVehicles = new();
        private List<int> mReservedPeople = new();
        private Random mRand = new();
        private RouteFinder mRouteFinder = new();
        private GraphMap mMap = new();
        private AgentBase mAgent;
        private TripRequestCoordinator mRequestCoordinator;
        private double mTimeStepInS = 5;
        private double mChargeRate = 0.034;
        private int mPopulation;
        private int mVehicleIdHelper;

        public event EventHandler TimeTick = delegate { };

        public WorldSimulation(Type agentType, double timeStep, string gmlFilePath, int startingCars = 0, int population = DEFAULT_POPULATION)
        {
            mPopulation = population;
            mTimeStepInS = timeStep;
            InitializeWorldMap(gmlFilePath);
            InitializeCars(startingCars);
            mRequestCoordinator = new(this, mMap, mTimeStepInS, 360);
            var agentInstance = Activator.CreateInstance(agentType, this, mRequestCoordinator, mRouteFinder, mMap);
            if (agentInstance == null)
                throw new Exception("Failed to create agent instance");
            mAgent = (AgentBase)agentInstance;
        }

        public double TimeStep { get { return mTimeStepInS; } }
        public double ChargeRate { get { return mChargeRate; } }
        public List<Vehicle> Vehicles { get { return mVehicles; } }

        private void IncrementTimeStep()
        {
            TimeTick(this, EventArgs.Empty);
        }

        private void InitializeCars(int numCars)
        {
            for (int i = 0; i < numCars; i++)
                RequestNewVehicle();
        }

        private void InitializeWorldMap(string gmlPath)
        {
            GraphMLParser.GraphMLParser parser = new();
            var nodesAndEdges = parser.ParseFile(gmlPath);
            var fileName = Path.GetFileNameWithoutExtension(gmlPath);
            mMap.SourceFileName = fileName;
            mRouteFinder = new(nodesAndEdges.Nodes, nodesAndEdges.Edges, mMap);
        }

        public int? ReserveAvailablePersonId()
        {
            var person = mRand.Next(mPopulation);
            while (mReservedPeople.Contains(person))
            {
                if (mReservedPeople.Count == mPopulation)
                    return null;
                person = mRand.Next(mPopulation);
            }
            return person;
        }

        public void FreePersonId(int personId)
        { mReservedPeople.Remove(personId);}

        public Vehicle RequestNewVehicle()
        {
            Console.WriteLine($"New vehicle requested, current total: {mVehicles.Count}");
            var loc = mMap.GetRandomValidLocation();
            var car = new Vehicle(mVehicleIdHelper, this, mMap, loc, VehicleStatus.Idle);
            mVehicleIdHelper++;
            mVehicles.Add(car);
            return car;
        }

        public void SimulateHours(double hours)
        {
            var start = DateTime.Now;
            var elapsedSteps = 0;
            var timeStepsToSimulate = hours * 3600 / mTimeStepInS;
            while (elapsedSteps < timeStepsToSimulate)
            {
                IncrementTimeStep();
                elapsedSteps++;
                var percentComplete = (elapsedSteps / timeStepsToSimulate) * 100;
                var elapsedMinutes = (DateTime.Now - start).TotalMinutes;
                var totalMinutesEstimate = elapsedMinutes / (percentComplete / 100);
                var remainingMinutes = totalMinutesEstimate - elapsedMinutes;
                Console.WriteLine($"{percentComplete}% simulated), time remaining: {Math.Floor(remainingMinutes / 60)}h{remainingMinutes%60}m");
            }

            Console.WriteLine($"-------------------------SIMULATION COMPLETE-------------------------");
            Console.WriteLine($"Total distance driven: {Vehicle.TotalDistanceKm}km");
            Console.WriteLine($"Total energy used: {Vehicle.TotalChargeUsed} units of charge");
            Console.WriteLine($"Total cars: {mVehicles.Count()}");
            Console.WriteLine($"Rides completed: {Vehicle.RidersTransported}");
            Console.WriteLine($"Rides in progress: {mAgent.TripsInProgress}");
            Console.WriteLine($"Total rider waiting time: {Math.Floor(mAgent.TimeSpentWaiting/3600)}:{mAgent.TimeSpentWaiting%60}");
            Console.WriteLine($"Average rider waiting time: {((mAgent.TimeSpentWaiting / 60) / Vehicle.RidersTransported)} min");
            Console.WriteLine($"Simulation took {(DateTime.Now - start)}");
        }
    }
}
