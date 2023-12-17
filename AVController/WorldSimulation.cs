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
        private RouteFinder mRouteFinder = new();
        private GraphMap mMap = new();
        private DispatchAgent mAgent;
        private TripRequestCoordinator mRequestCoordinator;
        private double mTimeStepInS = 5;
        private double mChargeRate = 0.034;
        private int mPopulation;
        private int mVehicleIdHelper;

        public event EventHandler TimeTick = delegate { };
        public WorldSimulation(double timeStep, string gmlFilePath, int population = DEFAULT_POPULATION)
        {
            mPopulation = population;
            mTimeStepInS = timeStep;
            InitializeWorldMap(gmlFilePath);
            mRequestCoordinator = new(this, mMap, mTimeStepInS, 360, mPopulation);
            mAgent = new(this, mRequestCoordinator, mRouteFinder, mMap);
        }

        public double TimeStep { get { return mTimeStepInS; } }
        public double ChargeRate { get { return mChargeRate; } }
        public List<Vehicle> Vehicles { get { return mVehicles; } }

        private void IncrementTimeStep()
        {
            TimeTick(this, EventArgs.Empty);
        }

        private void InitializeWorldMap(string gmlPath)
        {
            GraphMLParser.GraphMLParser parser = new();
            var nodesAndEdges = parser.ParseFile(gmlPath);
            var fileName = Path.GetFileNameWithoutExtension(gmlPath);
            mMap.SourceFileName = fileName;
            mRouteFinder = new(nodesAndEdges.Nodes, nodesAndEdges.Edges, mMap);
        }

        public Vehicle RequestNewVehicle()
        {
            var loc = mMap.GetRandomValidLocation();
            var car = new Vehicle(mVehicleIdHelper, this, mMap, loc, VehicleStatus.Idle);
            mVehicleIdHelper++;
            mVehicles.Add(car);
            return car;
        }

        public void BeginSimulation()
        {
            while (true)
            {
                IncrementTimeStep();
            }
        }
    }
}
