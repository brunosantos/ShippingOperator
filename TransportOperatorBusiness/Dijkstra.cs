using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    //TODO remove static...pass routes to ctor
    public static class Dijkstra
    {
        public static List<IRoute> GetShortestRoute(IPort source, IPort destination, IEnumerable<IRoute> routes)
        {
            if (source == destination)                            
                return GetShortestRouteBetweenSelf(source, routes);            
            else
                return  GetShortestRoutes(source, routes)[destination];

        }

        public static int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, List<IRoute> routes, int maxNumberOfStops)
        {
            var result1 = BreadthFirstSearchRoutesWithRepetition(source, routes, maxNumberOfStops);
            int numberOfRoutes = 0;

            if (result1.Count < maxNumberOfStops)
                maxNumberOfStops = result1.Count-1;
            for (int i = maxNumberOfStops; i >= 0; --i)
            {
                var x = result1[i].Count(r => r.Destination == destination);
                numberOfRoutes += x;
            }

            return numberOfRoutes; //routeDictionary.Values.ToList().FindAll(r => r.Count <= maxNumberOfStops && r.Count > 0);
        }

        public static IEnumerable<IEnumerable<IPort>> BreadthFirstSearch(IPort start, IPort end, List<IRoute> routes)
        {
            var adjacentRoutes = GetAdjacentRoutesPerNode(routes);
            var nodesPerLevel = new List<IEnumerable<IPort>>();
            var visitedNodes = new List<IPort>();
            var queue = new Queue<IPort>();
            queue.Enqueue(start);
            visitedNodes.Add(start);
            nodesPerLevel.Add(new List<IPort>() {start});
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                //visitedNodes.Add(currentNode);
                
                var levelOfNodes = new List<IPort>();
                foreach (var child in adjacentRoutes[currentNode].Select(x=>x.Destination))
                {
                    if (!visitedNodes.Contains(child))
                    {
                        visitedNodes.Add(child);
                        levelOfNodes.Add(child);
                        if (child == end)
                        {
                            //I have to save routes instead of nodes.
                            //and do visitedroutes.contains(childroute)...
                            nodesPerLevel.Add(levelOfNodes);
                            return nodesPerLevel;
                            //return true;
                        }
                        queue.Enqueue(child);  
                    }
                    //queue.Enqueue(child);                                        
                }

                if (levelOfNodes.Count>0)
                    nodesPerLevel.Add(levelOfNodes);
            }
            return nodesPerLevel;
        }


        public static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutes(IPort start, List<IRoute> routes)
        {
            return BreadthFirstSearchRoutes(start, routes, Int32.MaxValue);
        }

        public static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutes(IPort start, List<IRoute> routes, int maxNumberOfLevels)
        {
            //Dictionary is zero based.
            //maxNumberOfLevels -= 1;
            var routesPerLevel = new Dictionary<int, List<IRoute>>();   
            int level = 0;

            var queue = new Queue<KeyValuePair <int,IPort>>();
            queue.Enqueue(new KeyValuePair<int, IPort>(level, start));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                //if (maxNumberOfLevels == 0 || currentNode.Value == end)
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

        public static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutess(IPort start, IPort end, List<IRoute> routes, int level, int numberOfLevels)
        {
            var routesPerLevel = new Dictionary<int, List<IRoute>>();            

            var queue = new Queue<IPort>();
            queue.Enqueue(start);
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                //visitedNodes.Add(currentNode);

                if (numberOfLevels == 0 || currentNode == end)
                {
                    return routesPerLevel;
                }

                var visitedRoutes = new List<IRoute>();
                var adjacentRoutes = GetAdjacentRoutes(currentNode, routes);

                routesPerLevel.Add(level, (List<IRoute>)adjacentRoutes);

                
                foreach (var route in adjacentRoutes)
                {
                    //var visitedRoutes = GetVisitedRoutes(routesPerLevel);

                    if (!visitedRoutes.Contains(route))
                    {
                        //visitedRoutes.Add(route);
                        //BreadthFirstSearchRoute(route.Destination, end, routes, --maxNumberOfLevels);
                        //BreadthFirstSearchRoute(route.Destination, end, routes);
                        var nextroutes = BreadthFirstSearchRoutess(route.Destination, end, routes, level+1, numberOfLevels);
                        foreach (var nextroutess in nextroutes)
                        {
                            if (routesPerLevel.Count > nextroutess.Key)
                                routesPerLevel[level+1].AddRange(nextroutess.Value);
                            else
                                routesPerLevel.Add(level+1, nextroutess.Value);
                        }
                        queue.Enqueue(route.Destination);
                    }
                }

                //need to add to a specific node.                                
                //routesPerLevel = addRouteToLevel(visitedRoutes, level);
                if (routesPerLevel.Count > level)
                    routesPerLevel[level].AddRange(visitedRoutes);
                else
                    routesPerLevel.Add(level, visitedRoutes);   
                    

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

        //private static List<IEnumerable<IRoute>> addRouteToLevel(List<IRoute> visitedRoutes, int level)
        //{
        //    visitedRoutes[level] 
        //}

        public static IEnumerable<IRoute> BreadthFirstSearchRoute(IPort start, List<IRoute> routes)
        {            
            var visitedRoutes = new List<IRoute>();
            var adjacentRoutes = GetAdjacentRoutes(start, routes);
            foreach (var route in adjacentRoutes)
            {
                visitedRoutes.Add(route);                
            }            
            return visitedRoutes;
        }

        private static IEnumerable<IRoute> GetAdjacentRoutes(IPort port, List<IRoute> routes)
        {
            return routes.Where(n => n.Origin == port);
        }

        //public static IEnumerable<IPort> BreadthFirstTopDownTraversal(IPort root, Func<IPort, IEnumerable<IPort>> children)
        //{
        //    var q = new Queue<IPort>();
        //    q.Enqueue(root);
        //    while (q.Count > 0)
        //    {
        //        IPort current = q.Dequeue();
        //        yield return current;
        //        foreach (var child in children(current))
        //            q.Enqueue(child);
        //    }
        //}



        private static Dictionary<IPort, IEnumerable<IRoute>> GetAdjacentRoutesPerNode(IEnumerable<IRoute> routes)
        {            
            return routes.Select(r => r.Origin)
                                    .Distinct()
                                    .ToDictionary(node => node, node => routes.Where(n => n.Origin == node));
       }

        public static int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, List<IRoute> routes, int numberOfStops)
        {

            var result = BreadthFirstSearchRoutesWithRepetition(source, routes, numberOfStops);

            return result[numberOfStops].Count(r => r.Destination == destination);

            //var adjacentNodes = GetAdjacentNodes(source, routes);
            //Dictionary<IPort, LinkedList<IRoute>> routeDictionary;

            //if (source == destination)
            //    return null;
            //else
            //    routeDictionary = GetShortestRoutes(source, routes);

            //return routeDictionary.Values.ToList().FindAll(r => r.Count == numberOfStops && r.Count > 0);


        }

        //private static List<IJourney> BreadthFirstSearchJourneysWithRepetition(IPort start, IPort end, int maxJourneyTime, List<IRoute> routes, IRouteRepository routeRepository)
        //{            
        //    var journeys = new List<IJourney>();

        //    var queue = new Queue<KeyValuePair<IJourney, IPort>>();
        //    queue.Enqueue(new KeyValuePair<IJourney, IPort>(null, start));
        //    while (queue.Count != 0)
        //    {
        //        var currentNode = queue.Dequeue();
        //        if (maxJourneyTime == 0 && queue.Count == 0 || currentNode.Key.GetTime(routeRepository) > 25)
        //        {
        //            //add this last port....
        //            return journeys;
        //        }

        //        var visitedRoutes = new List<IRoute>();
        //        var adjacentRoutes = GetAdjacentRoutes(currentNode.Value, routes);
        //        foreach (var route in adjacentRoutes)
        //        {
        //            if (route.Destination.Equals(end))
        //            {
        //                journeys.Add(currentNode.Key);
        //            }

        //            visitedRoutes.Add(route);
        //            queue.Enqueue(new KeyValuePair<IJourney, IPort>(level + 1, route.Destination));
        //        }

        //        if (visitedRoutes.Count > 0)
        //        {
        //            if (journeys.Count > currentNode.Key)
        //                journeys[currentNode.Key].AddRange(visitedRoutes);
        //            else
        //                journeys.Add(currentNode.Key, visitedRoutes);
        //        }
        //    }
        //    return journeys;
        //}


        private static Dictionary<int, List<IRoute>> BreadthFirstSearchRoutesWithRepetition(IPort start, List<IRoute> routes, int maxNumberOfLevels)
        {
            var routesPerLevel = new Dictionary<int, List<IRoute>>();
            int level = 0;

            var queue = new Queue<KeyValuePair<int, IPort>>();
            queue.Enqueue(new KeyValuePair<int, IPort>(level, start));
            while (queue.Count != 0)
            {
                var currentNode = queue.Dequeue();
                //if (maxNumberOfLevels == 0 || currentNode.Value == end)
                if (maxNumberOfLevels == 0 && queue.Count == 0 || currentNode.Key > maxNumberOfLevels)
                {
                    return routesPerLevel;
                }

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

        //public static LinkedList<IRoute> GetShortestRoute(IPort source, IPort destination, IEnumerable<IRoute> routes)
        //{
        //    return GetShortestRoutes(source, destination, routes);
        //}

        public static List<List<IRoute>> CalculateShortestRoutesBetweenPortsWithMaxStopss(IPort source, IPort destination, IEnumerable<IRoute> routes, int numberOfStops)
        {
            var routeDictionary = GetShortestRouteBetweenSelf(source, routes).ToList();
            return null; //routeDictionary.ToList().FindAll(r => r.Count <= numberOfStops && r.Count > 0);
        }

        private static List<IRoute> GetShortestRoutes(IPort source, IPort destination, IEnumerable<IRoute> routes)
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

        private static IEnumerable<IPort> GetUnprocessedShortestRoutesOrigins(Dictionary<IPort, KeyValuePair<int, List<IRoute>>> ShortestRoutes, List<IPort> LocationsProcessed)
        {
            return GetShortestRoutesOriginLocation(ShortestRoutes).Where(_location => !LocationsProcessed.Contains(_location));
        }

        private static IEnumerable<IPort> GetShortestRoutesOriginLocation(Dictionary<IPort, KeyValuePair<int, List<IRoute>>> ShortestRoutes)
        {
            return ShortestRoutes.OrderBy(p => p.Value.Key)
                                 .Select(p => p.Key).ToList();
        }        
    }

    public static class ExtensionMethod
    {
        /// <summary>
        /// Adds or Updates the dictionary to include the destination and its associated cost and complete Route (and param arrays make Routes easier to work with)
        /// </summary>
        public static void Set(this Dictionary<IPort, KeyValuePair<int, List<IRoute>>> dictionary, IPort destination, int cost, params IRoute[] routes)
        {
            var completeRoute = routes == null ? new List<IRoute>() : new List<IRoute>(routes);
            dictionary[destination] = new KeyValuePair<int, List<IRoute>>(cost, completeRoute);
        }

        //public static void Set(this Dictionary<int, List<IJourney>> dictionary, int level, IRoute route)
        //{

        //    if (dictionary.Count > level)
        //        journeysPerLevel[currentNode.Key].AddRange(route);
        //    else
        //        journeysPerLevel.Add(currentNode.Key, visitedRoutes);

        //    var completeRoute = routes == null ? new LinkedList<IRoute>() : new LinkedList<IRoute>(routes);
        //    dictionary[destination] = new KeyValuePair<int, LinkedList<IRoute>>(cost, completeRoute);
        //}
    }
}
