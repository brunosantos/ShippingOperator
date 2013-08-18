using System;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    //SHOULD this be called Graph!?!?
    public class Graph
    {
        private IRouteRepository<IPort> _routeRepository;
        private List<IRoute<IPort>> _routes;

        public Graph(IRouteRepository<IPort> routeRepository)
        {            
            _routeRepository = routeRepository;            
            _routes = _routeRepository.GetAllRoutes();

        }

        public int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, int numberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, int maxNumberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(IPort source, IPort destination, int maxJourneytime)
        {
            var bfsRoutes = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, maxJourneytime,
                (mTime, journey) => journey.GetTime(_routeRepository) >= mTime);
            return bfsRoutes.Count();
        }

        private IEnumerable<IRoute<IPort>> GetAdjacentRoutes(IPort port)
        {
            var x =_routes.Where(n => n.Origin == port);
            return x;
        }

        public List<IJourney<IPort>> BreadthFirstSearchRoutesWithPortRepetitionLambda(IPort start, IPort destination,
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
                ProcessAdjacentRoutes(destination, currentNode, resultRoutes, queue);
            }
            return resultRoutes;
        }

        private void ProcessAdjacentRoutes(IPort destination, KeyValuePair<IJourney<IPort>, IRoute<IPort>> currentNode,
            List<IJourney<IPort>> resultRoutes, Queue<KeyValuePair<IJourney<IPort>, IRoute<IPort>>> queue)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination);
            foreach (var route in adjacentRoutes)
            {
                var nextjourney = GetNextJourney(currentNode, route);

                ProcessJourney(destination, route, resultRoutes, nextjourney, queue);
            }
        }


        private void ProcessJourney(IPort destination, IRoute<IPort> route, List<IJourney<IPort>> resultRoutes, IJourney<IPort> nextjourney, Queue<KeyValuePair<IJourney<IPort>, IRoute<IPort>>> queue)
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

        private IJourney<IPort> GetNextJourney(KeyValuePair<IJourney<IPort>, IRoute<IPort>> currentNode, IRoute<IPort> route)
        {
            var nextjourney = (IJourney<IPort>)currentNode.Key.Clone();
            nextjourney.WithPort(route.Destination);
            return nextjourney;
        }        

        public IJourney<IPort> GetShortestRoute(IPort source, IPort destination)
        {
            if (source == destination)
            {
                return GetShortestRouteBetweenSelf(source);
            }
            else
            {
                return GetShortestRoutes(source)[destination];
            }
        }

        private IJourney<IPort> GetShortestRouteBetweenSelf(IPort source)
        {
            var routeDictionary = GetShortestRoutes(source);
            //TODO use Journey instead of route!
            var adjacentNodes = GetAdjacentNodes(source);
            IJourney<IPort> shortestJourney = null;
            foreach (var node in adjacentNodes)
            {
                var currentRoute = routeDictionary[node];

                if ((currentRoute.NumberOfStops() > 0 && shortestJourney == null) ||
                    shortestJourney != null && shortestJourney.GetTime(_routeRepository) > currentRoute.GetTime(_routeRepository))
                {
                    //currentRoute.Add(routes.Single(r => r.Origin.Equals(node) && r.Destination.Equals(source)));
                    currentRoute.WithPort(source);
                    shortestJourney = currentRoute.Clone() as IJourney<IPort>;
                }
            }
            return shortestJourney;
        }

        private IEnumerable<IPort> GetAdjacentNodes(IPort source)
        {
            return _routes.Where(p=>p.Destination.Equals(source)).Select(p=>p.Origin);
        }

        //Refactor this to use a Queue?        
        private Dictionary<IPort, IJourney<IPort>> GetShortestRoutes(IPort source)
        {
            //TODO rename Location with node to be generic                        
            var shortestRoutes = new Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>>();
            var locationsProcessed = new List<IPort>();

            SetInfinityToAllRoutes(shortestRoutes);

            // update cost for self-to-self as 0; no Route            
            shortestRoutes[source] = new KeyValuePair<int, IJourney<IPort>>(0, new Journey<IPort>().WithPort(source));

            var locationCount = shortestRoutes.Keys.Count;

            while (locationsProcessed.Count < locationCount)
            {
                var locationToProcess = GetLocationToProcess(shortestRoutes, locationsProcessed);

                if (locationToProcess == null)
                    return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);

                var selectedRoutes = _routes.Where(p => p.Origin.Equals(locationToProcess));
                foreach (Route<IPort> route in selectedRoutes)
                {
                    if (shortestRoutes[route.Destination].Key > route.RouteTimeInDays + shortestRoutes[route.Origin].Key)
                    {
                        //shortestRoutes.Set(
                        //    route.Destination,
                        //    route.RouteTimeInDays + shortestRoutes[route.Origin].Key,
                        //    shortestRoutes[route.Origin].Value.WithPort(route.Destination));
                        var journey = ((IJourney<IPort>)shortestRoutes[route.Origin].Value.Clone()).WithPort(route.Destination);                        
                        shortestRoutes[route.Destination] = new KeyValuePair<int, IJourney<IPort>>(route.RouteTimeInDays + shortestRoutes[route.Origin].Key, journey);

                    }
                }

                //Add the location to the list of processed locations
                locationsProcessed.Add(locationToProcess);
            }

            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
            //return ShortestRoutes[destination].Value;
        }

        private void SetInfinityToAllRoutes(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes)
        {
            _routes.SelectMany(p => new IPort[] {p.Origin, p.Destination})
                  .ToList()
                  .ForEach(s => shortestRoutes.Set(s, Infinity, null));
        }

        private IPort GetLocationToProcess(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes, List<IPort> locationsProcessed)
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

        private IEnumerable<IPort> GetUnprocessedShortestRoutesOrigins(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes, List<IPort> locationsProcessed)
        {
            return GetShortestRoutesOriginLocation(shortestRoutes).Where(location => !locationsProcessed.Contains(location));
        }

        private IEnumerable<IPort> GetShortestRoutesOriginLocation(Dictionary<IPort, KeyValuePair<int, IJourney<IPort>>> shortestRoutes)
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
