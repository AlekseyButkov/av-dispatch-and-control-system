﻿using AVGraphPrimitives;
using Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    internal class DispatchAgent
    {
        GraphMap mWorldMap = new();
        RouteFinder mRouteFinder;
        public DispatchAgent(RouteFinder routeFinder, GraphMap map)
        {
            mRouteFinder = routeFinder;
            mWorldMap = map;
        }

        public void TestRoutes()
        {
            Stopwatch sw = new();
            while (true)
            {
                sw.Restart();
                mRouteFinder.TestRoutingBetweenRandomLocations();
                Console.WriteLine($"Found route in {sw.ElapsedMilliseconds}ms");
                Thread.Sleep(1000);
            }
        }
    }
}
