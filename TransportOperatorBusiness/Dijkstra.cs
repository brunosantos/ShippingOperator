using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    //TODO remove static...pass routes to ctor
    public static class Dijkstra
    {


        public static int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, List<IRoute> routes, int maxNumberOfStops)
        {
            var bfsRoutes = BreadthFirstSearchRoutesWithPortRepetition(source, destination, routes, maxNumberOfStops);
            return bfsRoutes.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public static int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(IPort source, IPort destination, List<IRoute> routes, int maxJourneytime)
        {
            var bfsRoutes = BreadthFirstSearchRoutesWithPortRepetition(source, destination, routes, maxJourneytime);
            return bfsRoutes.Count();
        }

        public static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutes(IPort start, List<IRoute> routes)
        {
            return BreadthFirstSearchRoutes(start, routes, Int32.MaxValue);
        }

        public static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutes(IPort start, List<IRoute> routes, int maxNumberOfLevels)
        {
            //Dictionary is zero based.
            var routesPerLevel = new Dictionary<int, List<IRoute>>();   
            int level = 0;

            var queue = new Queue<KeyValuePair <int,IPort>>();
            queue.Enqueue(new KeyValuePair<int, IPort>(level, start));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                if (maxNumberOfLevels == 0 && queue.Count == 0 || currentNode.Key > maxNumberOfLevels)
                {
                    return routesPerLevel;
                }
                var allVisitedRoutes = GetVisitedRoutes(routesPerLevel);
                var visitedRoutes = new List<IRoute>();
                var adjacentRoutes = GetAdjacentRoutes(currentNode.Value, routes);
                foreach (var route in adjacentRoutes)
                {
                    if (!allVisitedRoutes.Contains(route))
                    {
                        visitedRoutes.Add(route);
                        queue.Enqueue(new KeyValuePair<int, IPort>(level+1,route.Destination));
                    }
                }                
                if (visitedRoutes.Count > 0)
                {
                    if (routesPerLevel.Count > currentNode.Key)
                        routesPerLevel[currentNode.Key].AddRange(visitedRoutes);
                    else
                        routesPerLevel.Add(currentNode.Key, visitedRoutes);
                }
                ++level;
            }            
            return routesPerLevel;
        }        

        private static List<IRoute> GetVisitedRoutes(Dictionary<int, List<IRoute>> routesPerLevel)
        {
            var a = new List<IRoute>();
            foreach (var r in routesPerLevel.Values)
                a.AddRange(r);
            return a;
        }
        
        private static IEnumerable<IRoute> GetAdjacentRoutes(IPort port, IEnumerable<IRoute> routes)
        {
            return routes.Where(n => n.Origin == port);
        }

        public static int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, List<IRoute> routes, int numberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetition(source,destination, routes, numberOfStops);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }
        
        public static int GetNumberOfRoutesBetweenPortsWithNumberOfStopsv2(IPort source, IPort destination, List<IRoute> routes, int numberOfStops)
        {            
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda<IPort, IRoute, IJourney >(source,destination, routes, numberOfStops,(nEdjes,journey)=> journey.NumberOfStops() >= nEdjes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }        
       
        private static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutesWithPortRepetition(IPort start, List<IRoute> routes, int maxNumberOfLevels)
        {
            var routesPerLevel = new Dictionary<int, List<IRoute>>();
            int level = 0;

            var queue = new Queue<KeyValuePair<int, IPort>>();
            queue.Enqueue(new KeyValuePair<int, IPort>(level, start));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                if (maxNumberOfLevels == 0 && queue.Count == 0)
                {
                    return routesPerLevel;
                }

                if(currentNode.Key > maxNumberOfLevels)
                    break;

                var visitedRoutes = new List<IRoute>();
                var adjacentRoutes = GetAdjacentRoutes(currentNode.Value, routes);
                foreach (var route in adjacentRoutes)
                {
                    visitedRoutes.Add(route);
                    queue.Enqueue(new KeyValuePair<int, IPort>(level + 1, route.Destination));                    
                }

                if (visitedRoutes.Count > 0)
                {
                    if (routesPerLevel.Count > currentNode.Key)
                        routesPerLevel[currentNode.Key].AddRange(visitedRoutes);
                    else
                        routesPerLevel.Add(currentNode.Key, visitedRoutes);
                }
                ++level;
            }
            return routesPerLevel;
        }


        public static List<IJourney> BreadthFirstSearchRoutesWithPortRepetitionLambda<TNode, TEdge, TEdges>(IPort start, IPort destination, List<IRoute> routes,
            int maxNumberOfStops, Func<int, TEdges, bool> keySelector)
        {
            var portRepository = new PortRepository();
            var routeRepository = new RouteRepository(portRepository);

            var resultRoutes = new List<IJourney>();
            IJourney journey = new Journey().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney, IRoute>>();
            queue.Enqueue(new KeyValuePair<IJourney, IRoute>(journey, new Route(null, start, 0)));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();

                if (maxNumberOfStops == 0 && queue.Count == 0)
                {
                    return resultRoutes;
                }

                var currentjourney = currentNode.Key;
                if (lambda(maxNumberOfStops, currentjourney))
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
                        queue.Enqueue(new KeyValuePair<IJourney, IRoute>(visitedjourney, route));
                    }
                }
            }
            return resultRoutes;
        }

        private static bool lambda(int maxNumberOfStops, IJourney currentjourney)
        {
            return currentjourney.NumberOfStops() >= maxNumberOfStops;
        }


        public static List<IJourney> BreadthFirstSearchRoutesWithPortRepetition(IPort start, IPort destination, List<IRoute> routes, int maxNumberOfStops)
        {
            var portRepository = new PortRepository();
            var routeRepository = new RouteRepository(portRepository);

            var resultRoutes = new List<IJourney>();
            IJourney journey = new Journey().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney, IRoute>>();
            queue.Enqueue(new KeyValuePair<IJourney, IRoute>(journey, new Route(null, start, 0)));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();

                if (maxNumberOfStops == 0 && queue.Count == 0)
                {
                    return resultRoutes;
                }

                if (currentNode.Key.NumberOfStops() >= maxNumberOfStops)
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
                        queue.Enqueue(new KeyValuePair<IJourney, IRoute>(visitedjourney, route));
                    }
                }
            }
            return resultRoutes;
        }

        public static List<IRoute> GetShortestRoute(IPort source, IPort destination, IEnumerable<IRoute> routes)
        {
            if (source == destination)
                return GetShortestRouteBetweenSelf(source, routes);
            else
                return GetShortestRoutes(source, routes)[destination];
        }

        private static List<IRoute> GetShortestRouteBetweenSelf(IPort source, IEnumerable<IRoute> routes)
        {
            var routeDictionary = GetShortestRoutes(source, routes);
            //TODO use Journey instead of route!
            var adjacentNodes = GetAdjacentNodes(source, routes);
            List<IRoute> shortestRoute = null;
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

        private static List<IRoute> GetShortestRouteBetweenSelfRefactored(IPort source, IEnumerable<IRoute> routes)
        {
            var routeDictionary = GetShortestRoutes(source, routes);
            //TODO use Journey instead of route!
            var adjacentNodes = GetAdjacentNodes(source, routes);
            List<IRoute> shortestRoute = null;
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

        private static IEnumerable<IPort> GetAdjacentNodes(IPort source, IEnumerable<IRoute> routes)
        {
            return routes.Where(p=>p.Destination.Equals(source)).Select(p=>p.Origin);
        }

        //Refactor this to use a Queue?
        private static Dictionary<IPort, List<IRoute>> GetShortestRoutes(IPort source, IEnumerable<IRoute> routes)
        {
            //TODO rename Location with node to be generic                        
            var shortestRoutes = new Dictionary<IPort, KeyValuePair<int, List<IRoute>>>();            
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
                foreach (Route route in selectedRoutes)
                {
                    if (shortestRoutes[route.Destination].Key > route.RouteTimeInDays + shortestRoutes[route.Origin].Key)
                    {
                        shortestRoutes.Set(
                            route.Destination,
                            route.RouteTimeInDays + shortestRoutes[route.Origin].Key,
                            shortestRoutes[route.Origin].Value.Union(new IRoute[] { route }).ToArray());
                    }
                } 

                //Add the location to the list of processed locations
                locationsProcessed.Add(locationToProcess);
            } 

            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
            //return ShortestRoutes[destination].Value;
        }

        private static void SetInfinityToAllRoutes(IEnumerable<IRoute> routes, Dictionary<IPort, KeyValuePair<int, List<IRoute>>> shortestRoutes)
        {
            routes.SelectMany(p => new IPort[] {p.Origin, p.Destination})
                  .ToList()
                  .ForEach(s => shortestRoutes.Set(s, Infinity, null));
        }

        private static IPort GetLocationToProcess(Dictionary<IPort, KeyValuePair<int, List<IRoute>>> shortestRoutes, 
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

        private static IEnumerable<IPort> GetUnprocessedShortestRoutesOrigins(Dictionary<IPort, KeyValuePair<int, List<IRoute>>> shortestRoutes, List<IPort> locationsProcessed)
        {
            return GetShortestRoutesOriginLocation(shortestRoutes).Where(location => !locationsProcessed.Contains(location));
        }

        private static IEnumerable<IPort> GetShortestRoutesOriginLocation(Dictionary<IPort, KeyValuePair<int, List<IRoute>>> shortestRoutes)
        {
            return shortestRoutes.OrderBy(p => p.Value.Key)
                                 .Select(p => p.Key).ToList();
        }
    }

    public static class ExtensionMethod
    {
        public static void Set(this Dictionary<IPort, KeyValuePair<int, List<IRoute>>> dictionary, IPort destination, int cost, params IRoute[] routes)
        {
            var completeRoute = routes == null ? new List<IRoute>() : new List<IRoute>(routes);
            dictionary[destination] = new KeyValuePair<int, List<IRoute>>(cost, completeRoute);
        }
    }
}
