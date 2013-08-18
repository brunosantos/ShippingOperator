using System;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    //TODO remove static...pass routes to ctor
    //SHOULD this be called Graph!?!?
    public static class Dijkstra
    {
        public static int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, List<IRoute<IPort>> routes, int numberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, routes, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public static int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, List<IRoute<IPort>> routes, int maxNumberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, routes, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public static int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(IPort source, IPort destination, List<IRoute<IPort>> routes, int maxJourneytime)
        {
            var portRepository = new PortRepository();
            var routeRepository = new RouteRepository(portRepository);

            var bfsRoutes = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, routes, maxJourneytime,
                (mTime, journey) => journey.GetTime(routeRepository) >= mTime);
            return bfsRoutes.Count();
        }

        private static IEnumerable<IRoute<IPort>> GetAdjacentRoutes(IPort port, IEnumerable<IRoute<IPort>> routes)
        {
            return routes.Where(n => n.Origin == port);
        }


        public static List<IJourney<IPort>> BreadthFirstSearchRoutesWithPortRepetitionLambda(IPort start, IPort destination, List<IRoute<IPort>> routes,
            int maxNumberOfStops, Func<int, IJourney<IPort>, bool> journeyComparer)
        {
            var resultRoutes = new List<IJourney<IPort>>();
            IJourney<IPort> journey = new Journey<IPort>().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney<IPort>, IRoute<IPort>>>();
            queue.Enqueue(new KeyValuePair<IJourney<IPort>, IRoute<IPort>>(journey, new Route<IPort>(null, start, 0)));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();

                //could this scope be an async call !?!?
                if (maxNumberOfStops == 0 && queue.Count == 0)
                {
                    return resultRoutes;
                }

                var currentjourney = currentNode.Key;
                if (journeyComparer(maxNumberOfStops, currentjourney))
                    break;

                //this can be an async call.
                ProcessAdjacentRoutes(destination, routes, currentNode, resultRoutes, queue);
            }
            return resultRoutes;
        }

        private static void ProcessAdjacentRoutes(IPort destination, IEnumerable<IRoute<IPort>> routes, KeyValuePair<IJourney<IPort>, IRoute<IPort>> currentNode,
            List<IJourney<IPort>> resultRoutes, Queue<KeyValuePair<IJourney<IPort>, IRoute<IPort>>> queue)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination, routes);
            foreach (var route in adjacentRoutes)
            {
                var nextjourney = GetNextJourney(currentNode, route);

                ProcessJourney(destination, route, resultRoutes, nextjourney, queue);
            }
        }

        private static void ProcessJourney(IPort destination, IRoute<IPort> route, List<IJourney<IPort>> resultRoutes, IJourney<IPort> nextjourney, Queue<KeyValuePair<IJourney<IPort>, IRoute<IPort>>> queue)
        {
            if (route.Destination.Equals(destination))
            {
                resultRoutes.Add(nextjourney);
            }
            else
            {
                queue.Enqueue(new KeyValuePair<IJourney<IPort>, IRoute<IPort>>(nextjourney, route));
            }
        }

        private static IJourney<IPort> GetNextJourney(KeyValuePair<IJourney<IPort>, IRoute<IPort>> currentNode, IRoute<IPort> route)
        {
            var nextjourney = (IJourney<IPort>)currentNode.Key.Clone();
            nextjourney.WithPort(route.Destination);
            return nextjourney;
        }        

        //public static List<IRoute<IPort>> GetShortestRoute(IPort source, IPort destination, IEnumerable<IRoute<IPort>> routes)
        //{
        //    if (source == destination)
        //    {
        //        var x = GetShortestRouteBetweenSelf(source, routes);
        //        return null;
        //    }
        //    else
        //    {
        //        return GetShortestRoutes(source, routes)[destination];
        //    }
        //}


        public static IJourney<IPort> GetShortestRoute(IPort source, IPort destination, IEnumerable<IRoute<IPort>> routes)
        {
            if (source == destination)
            {
                return GetShortestRouteBetweenSelf(source, routes);
            }
            else
            {
                return GetShortestRoutes(source, routes)[destination];
            }
        }

        private static IJourney<IPort> GetShortestRouteBetweenSelf(IPort source, IEnumerable<IRoute<IPort>> routes)
        {
            var portRepository = new PortRepository();
            var routeRepository = new RouteRepository(portRepository);

            var routeDictionary = GetShortestRoutes(source, routes);
            //TODO use Journey instead of route!
            var adjacentNodes = GetAdjacentNodes(source, routes);
            IJourney<IPort> shortestJourney = null;
            foreach (var node in adjacentNodes)
            {
                var currentRoute = routeDictionary[node];

                if ((currentRoute.NumberOfStops() > 0 && shortestJourney == null) ||
                    shortestJourney != null && shortestJourney.GetTime(routeRepository) > currentRoute.GetTime(routeRepository))
                {
                    //currentRoute.Add(routes.Single(r => r.Origin.Equals(node) && r.Destination.Equals(source)));
                    currentRoute.WithPort(source);
                    shortestJourney = currentRoute.Clone() as IJourney<IPort>;
                }
            }
            return shortestJourney;
        }

        private static IEnumerable<IPort> GetAdjacentNodes(IPort source, IEnumerable<IRoute<IPort>> routes)
        {
            return routes.Where(p=>p.Destination.Equals(source)).Select(p=>p.Origin);
        }

        //Refactor this to use a Queue?
        //private static Dictionary<IPort, List<IRoute<IPort>>> GetShortestRoutes(IPort source, IEnumerable<IRoute<IPort>> routes)
        //{
        //    //TODO rename Location with node to be generic                        
        //    var shortestRoutes = new Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>>();            
        //    var locationsProcessed = new List<IPort>();
            
        //    SetInfinityToAllRoutes(routes, shortestRoutes);

        //    // update cost for self-to-self as 0; no Route
        //    shortestRoutes.Set(source, 0, null);
            
        //    var locationCount = shortestRoutes.Keys.Count;

        //    while (locationsProcessed.Count < locationCount)
        //    {
        //        var locationToProcess = GetLocationToProcess(shortestRoutes, locationsProcessed);

        //        if (locationToProcess == null)
        //            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);

        //        var selectedRoutes = routes.Where(p => p.Origin.Equals(locationToProcess));
        //        foreach (Route<IPort> route in selectedRoutes)
        //        {
        //            if (shortestRoutes[route.Destination].Key > route.RouteTimeInDays + shortestRoutes[route.Origin].Key)
        //            {
        //                shortestRoutes.Set(
        //                    route.Destination,
        //                    route.RouteTimeInDays + shortestRoutes[route.Origin].Key,
        //                    shortestRoutes[route.Origin].Value.Union(new IRoute<IPort>[] { route }).ToArray());
        //            }
        //        } 

        //        //Add the location to the list of processed locations
        //        locationsProcessed.Add(locationToProcess);
        //    } 

        //    return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
        //    //return ShortestRoutes[destination].Value;
        //}

        private static Dictionary<IPort, IJourney<IPort>> GetShortestRoutes(IPort source, IEnumerable<IRoute<IPort>> routes)
        {
            //TODO rename Location with node to be generic                        
            var shortestRoutes = new Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>>();
            var locationsProcessed = new List<IPort>();

            SetInfinityToAllRoutes(routes, shortestRoutes);

            // update cost for self-to-self as 0; no Route
            //shortestRoutes.Set(source, 0, null);
            shortestRoutes[source] = new KeyValuePair<int, IJourney<IPort>>(0, new Journey<IPort>().WithPort(source));


            var locationCount = shortestRoutes.Keys.Count;

            while (locationsProcessed.Count < locationCount)
            {
                var locationToProcess = GetLocationToProcess(shortestRoutes, locationsProcessed);

                if (locationToProcess == null)
                    return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);

                var selectedRoutes = routes.Where(p => p.Origin.Equals(locationToProcess));
                foreach (Route<IPort> route in selectedRoutes)
                {
                    if (shortestRoutes[route.Destination].Key > route.RouteTimeInDays + shortestRoutes[route.Origin].Key)
                    {
                        //shortestRoutes.Set(
                        //    route.Destination,
                        //    route.RouteTimeInDays + shortestRoutes[route.Origin].Key,
                        //    shortestRoutes[route.Origin].Value.WithPort(route.Destination));
                        var journey = ((IJourney<IPort>)shortestRoutes[route.Origin].Value.Clone()).WithPort(route.Destination);
                        var completeRoute = journey ?? new Journey<IPort>();
                        shortestRoutes[route.Destination] = new KeyValuePair<int, IJourney<IPort>>(route.RouteTimeInDays + shortestRoutes[route.Origin].Key, completeRoute);

                    }
                }

                //Add the location to the list of processed locations
                locationsProcessed.Add(locationToProcess);
            }

            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
            //return ShortestRoutes[destination].Value;
        }

        private static void SetInfinityToAllRoutes(IEnumerable<IRoute<IPort>> routes, Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes)
        {
            routes.SelectMany(p => new IPort[] {p.Origin, p.Destination})
                  .ToList()
                  .ForEach(s => shortestRoutes.Set(s, Infinity, null));
        }

        private static IPort GetLocationToProcess(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes, 
            List<IPort> locationsProcessed)
        {
            var unprocessedShortestRouteOrigin = GetUnprocessedShortestRoutesOrigins(shortestRoutes, locationsProcessed).First();
            var shortestRouteCost = shortestRoutes[unprocessedShortestRouteOrigin].Key;
            if (shortestRouteCost == Infinity)
                return null;

            return unprocessedShortestRouteOrigin; 
        }

        private static int Infinity
        {
            get { return Int32.MaxValue; }
        }

        private static IEnumerable<IPort> GetUnprocessedShortestRoutesOrigins(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes, List<IPort> locationsProcessed)
        {
            return GetShortestRoutesOriginLocation(shortestRoutes).Where(location => !locationsProcessed.Contains(location));
        }

        private static IEnumerable<IPort> GetShortestRoutesOriginLocation(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes)
        {
            return shortestRoutes.OrderBy(p => p.Value.Key)
                                 .Select(p => p.Key).ToList();
        }
    }

    public static class ExtensionMethod
    {
        public static void Set(this Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> dictionary, IPort destination, int cost, IJourney<IPort> journey)
        {
            var completeRoute = journey ?? new Journey<IPort>();
            dictionary[destination] = new KeyValuePair<int, IJourney<IPort>>(cost, completeRoute);
        }
    }
}
