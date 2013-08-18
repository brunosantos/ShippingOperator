using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    //TODO remove static...pass routes to ctor
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


        public static List<IJourney> BreadthFirstSearchRoutesWithPortRepetitionLambda(IPort start, IPort destination, List<IRoute<IPort>> routes,
            int maxNumberOfStops, Func<int, IJourney, bool> journeyComparer)
        {
            var resultRoutes = new List<IJourney>();
            IJourney journey = new Journey().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney, IRoute<IPort>>>();
            queue.Enqueue(new KeyValuePair<IJourney, IRoute<IPort>>(journey, new Route<IPort>(null, start, 0)));
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

        private static void ProcessAdjacentRoutes(IPort destination, IEnumerable<IRoute<IPort>> routes, KeyValuePair<IJourney, IRoute<IPort>> currentNode,
            List<IJourney> resultRoutes, Queue<KeyValuePair<IJourney, IRoute<IPort>>> queue)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination, routes);
            foreach (var route in adjacentRoutes)
            {
                var nextjourney = GetNextJourney(currentNode, route);

                ProcessJourney(destination, route, resultRoutes, nextjourney, queue);
            }
        }

        private static void ProcessJourney(IPort destination, IRoute<IPort> route, List<IJourney> resultRoutes, IJourney nextjourney, Queue<KeyValuePair<IJourney, IRoute<IPort>>> queue)
        {
            if (route.Destination.Equals(destination))
            {
                resultRoutes.Add(nextjourney);
            }
            else
            {
                queue.Enqueue(new KeyValuePair<IJourney, IRoute<IPort>>(nextjourney, route));
            }
        }

        private static IJourney GetNextJourney(KeyValuePair<IJourney, IRoute<IPort>> currentNode, IRoute<IPort> route)
        {
            var nextjourney = (IJourney) currentNode.Key.Clone();
            nextjourney.WithPort(route.Destination);
            return nextjourney;
        }

        public static List<IJourney> BreadthFirstSearchRoutesWithPortRepetitionOld(IPort start, IPort destination, List<IRoute<IPort>> routes,
            int maxNumberOfStops, Func<int, IJourney, bool> journeyComparer)
        {
            var resultRoutes = new List<IJourney>();
            IJourney journey = new Journey().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney, IRoute<IPort>>>();
            queue.Enqueue(new KeyValuePair<IJourney, IRoute<IPort>>(journey, new Route<IPort>(null, start, 0)));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();

                if (maxNumberOfStops == 0 && queue.Count == 0)
                {
                    return resultRoutes;
                }

                var currentjourney = currentNode.Key;
                if (journeyComparer(maxNumberOfStops, currentjourney))
                    break;

                var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination, routes);
                foreach (var route in adjacentRoutes)
                {
                    var visitedjourney = (IJourney)currentNode.Key.Clone();
                    visitedjourney.WithPort(route.Destination);

                    if (route.Destination.Equals(destination))
                    {
                        resultRoutes.Add(visitedjourney);
                    }
                    else
                    {
                        queue.Enqueue(new KeyValuePair<IJourney, IRoute<IPort>>(visitedjourney, route));
                    }
                }
            }
            return resultRoutes;
        }

        public static List<IRoute<IPort>> GetShortestRoute(IPort source, IPort destination, IEnumerable<IRoute<IPort>> routes)
        {
            if (source == destination)
                return GetShortestRouteBetweenSelf(source, routes);
            else
                return GetShortestRoutes(source, routes)[destination];
        }

        private static List<IRoute<IPort>> GetShortestRouteBetweenSelf(IPort source, IEnumerable<IRoute<IPort>> routes)
        {
            var routeDictionary = GetShortestRoutes(source, routes);
            //TODO use Journey instead of route!
            var adjacentNodes = GetAdjacentNodes(source, routes);
            List<IRoute<IPort>> shortestRoute = null;
            foreach (var node in adjacentNodes)
            {
                var currentRoute = routeDictionary[node];
                var x = routes.Select(r => r.Origin.Equals(source) && r.Destination.Equals(node));

                //TODO refactor code repetition!
                if (shortestRoute == null)
                {
                    if (currentRoute.Count > 0)
                    {
                        currentRoute.Add(routes.Single(r => r.Origin.Equals(node) && r.Destination.Equals(source)));
                        shortestRoute = currentRoute;
                    }
                }
                else
                {
                    if (shortestRoute.Sum(r => r.RouteTimeInDays) > currentRoute.Sum(s => s.RouteTimeInDays))
                    {
                        currentRoute.Add(routes.Single(r => r.Origin.Equals(node) && r.Destination.Equals(source)));
                        shortestRoute = currentRoute;
                    }
                }
            }
            return shortestRoute;
        }

        private static IEnumerable<IPort> GetAdjacentNodes(IPort source, IEnumerable<IRoute<IPort>> routes)
        {
            return routes.Where(p=>p.Destination.Equals(source)).Select(p=>p.Origin);
        }

        //Refactor this to use a Queue?
        private static Dictionary<IPort, List<IRoute<IPort>>> GetShortestRoutes(IPort source, IEnumerable<IRoute<IPort>> routes)
        {
            //TODO rename Location with node to be generic                        
            var shortestRoutes = new Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>>();            
            var locationsProcessed = new List<IPort>();
            
            SetInfinityToAllRoutes(routes, shortestRoutes);

            // update cost for self-to-self as 0; no Route
            shortestRoutes.Set(source, 0, null);
            
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
                        shortestRoutes.Set(
                            route.Destination,
                            route.RouteTimeInDays + shortestRoutes[route.Origin].Key,
                            shortestRoutes[route.Origin].Value.Union(new IRoute<IPort>[] { route }).ToArray());
                    }
                } 

                //Add the location to the list of processed locations
                locationsProcessed.Add(locationToProcess);
            } 

            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
            //return ShortestRoutes[destination].Value;
        }

        private static void SetInfinityToAllRoutes(IEnumerable<IRoute<IPort>> routes, Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>> shortestRoutes)
        {
            routes.SelectMany(p => new IPort[] {p.Origin, p.Destination})
                  .ToList()
                  .ForEach(s => shortestRoutes.Set(s, Infinity, null));
        }

        private static IPort GetLocationToProcess(Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>> shortestRoutes, 
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

        private static IEnumerable<IPort> GetUnprocessedShortestRoutesOrigins(Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>> shortestRoutes, List<IPort> locationsProcessed)
        {
            return GetShortestRoutesOriginLocation(shortestRoutes).Where(location => !locationsProcessed.Contains(location));
        }

        private static IEnumerable<IPort> GetShortestRoutesOriginLocation(Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>> shortestRoutes)
        {
            return shortestRoutes.OrderBy(p => p.Value.Key)
                                 .Select(p => p.Key).ToList();
        }
    }

    public static class ExtensionMethod
    {
        public static void Set(this Dictionary<IPort, KeyValuePair<int, List<IRoute<IPort>>>> dictionary, IPort destination, int cost, params IRoute<IPort>[] routes)
        {
            var completeRoute = routes == null ? new List<IRoute<IPort>>() : new List<IRoute<IPort>>(routes);
            dictionary[destination] = new KeyValuePair<int, List<IRoute<IPort>>>(cost, completeRoute);
        }
    }
}
