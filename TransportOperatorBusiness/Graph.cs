using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportOperatorBusiness
{
    public class Graph<TNode>
    {
        private IRouteRepository<TNode> _routeRepository;
        private List<IRoute<TNode>> _routes;

        public Graph(IRouteRepository<TNode> routeRepository)
        {            
            _routeRepository = routeRepository;            
            _routes = _routeRepository.GetAllRoutes();

        }

        public int GetNumberOfRoutesBetweenPortsWithNumberOfStops(TNode source, TNode destination, int numberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(TNode source, TNode destination, int maxNumberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(TNode source, TNode destination, int maxJourneytime)
        {
            var bfsRoutes = BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, maxJourneytime,
                (mTime, journey) => journey.GetTime(_routeRepository) >= mTime);
            return bfsRoutes.Count();
        }

        private IEnumerable<IRoute<TNode>> GetAdjacentRoutes(TNode port)
        {
            var x =_routes.Where(n => n.Origin.Equals(port));
            return x;
        }

        public List<IJourney<TNode>> BreadthFirstSearchRoutesWithPortRepetitionLambda(TNode start, TNode destination,
            int maxNumberOfStops, Func<int, IJourney<TNode>, bool> journeyComparer)
        {
            var resultRoutes = new List<IJourney<TNode>>();
            IJourney<TNode> journey = new Journey<TNode>().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>>();
            queue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(journey, new Route<TNode>(default(TNode), start, 0)));
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

        private void ProcessAdjacentRoutes(TNode destination, KeyValuePair<IJourney<TNode>, IRoute<TNode>> currentNode,
            List<IJourney<TNode>> resultRoutes, Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>> queue)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination);
            foreach (var route in adjacentRoutes)
            {
                var nextjourney = GetNextJourney(currentNode, route);

                ProcessJourney(destination, route, resultRoutes, nextjourney, queue);
            }
        }


        private void ProcessJourney(TNode destination, IRoute<TNode> route, List<IJourney<TNode>> resultRoutes, IJourney<TNode> nextjourney, Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>> queue)
        {
            if (route.Destination.Equals(destination))
            {
                resultRoutes.Add(nextjourney);
            }
            else
            {
                queue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(nextjourney, route));
            }
        }

        private IJourney<TNode> GetNextJourney(KeyValuePair<IJourney<TNode>, IRoute<TNode>> currentNode, IRoute<TNode> route)
        {
            var nextjourney = (IJourney<TNode>)currentNode.Key.Clone();
            nextjourney.WithPort(route.Destination);
            return nextjourney;
        }

        public IJourney<TNode> GetShortestRoute(TNode source, TNode destination)
        {
            if (source.Equals(destination))
            {
                return GetShortestRouteBetweenSelf(source);
            }
            else
            {
                return GetShortestRoutes(source)[destination];
            }
        }

        private IJourney<TNode> GetShortestRouteBetweenSelf(TNode source)
        {
            var routeDictionary = GetShortestRoutes(source);
            //TODO use Journey instead of route!
            var adjacentNodes = GetAdjacentNodes(source);
            IJourney<TNode> shortestJourney = null;
            foreach (var node in adjacentNodes)
            {
                var currentRoute = routeDictionary[node];

                if ((currentRoute.NumberOfStops() > 0 && shortestJourney == null) ||
                    shortestJourney != null && shortestJourney.GetTime(_routeRepository) > currentRoute.GetTime(_routeRepository))
                {
                    //currentRoute.Add(routes.Single(r => r.Origin.Equals(node) && r.Destination.Equals(source)));
                    currentRoute.WithPort(source);
                    shortestJourney = currentRoute.Clone() as IJourney<TNode>;
                }
            }
            return shortestJourney;
        }

        private IEnumerable<TNode> GetAdjacentNodes(TNode source)
        {
            return _routes.Where(p=>p.Destination.Equals(source)).Select(p=>p.Origin);
        }

        //Refactor this to use a Queue?        
        private Dictionary<TNode, IJourney<TNode>> GetShortestRoutes(TNode source)
        {
            //TODO rename Location with node to be generic                        
            var shortestRoutes = new Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>>();
            var locationsProcessed = new List<TNode>();

            SetInfinityToAllRoutes(shortestRoutes);

            // update cost for self-to-self as 0; no Route            
            shortestRoutes[source] = new KeyValuePair<int, IJourney<TNode>>(0, new Journey<TNode>().WithPort(source));

            var locationCount = shortestRoutes.Keys.Count;

            while (locationsProcessed.Count < locationCount)
            {
                var locationToProcess = GetLocationToProcess(shortestRoutes, locationsProcessed);

                if (locationToProcess == null)
                    return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);

                var selectedRoutes = _routes.Where(p => p.Origin.Equals(locationToProcess));
                foreach (Route<TNode> route in selectedRoutes)
                {
                    if (shortestRoutes[route.Destination].Key > route.RouteTimeInDays + shortestRoutes[route.Origin].Key)
                    {
                        //shortestRoutes.Set(
                        //    route.Destination,
                        //    route.RouteTimeInDays + shortestRoutes[route.Origin].Key,
                        //    shortestRoutes[route.Origin].Value.WithPort(route.Destination));
                        var journey = ((IJourney<TNode>)shortestRoutes[route.Origin].Value.Clone()).WithPort(route.Destination);
                        shortestRoutes[route.Destination] = new KeyValuePair<int, IJourney<TNode>>(route.RouteTimeInDays + shortestRoutes[route.Origin].Key, journey);

                    }
                }

                //Add the location to the list of processed locations
                locationsProcessed.Add(locationToProcess);
            }

            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
            //return ShortestRoutes[destination].Value;
        }

        private void SetInfinityToAllRoutes(Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> shortestRoutes)
        {
            _routes.SelectMany(p => new TNode[] { p.Origin, p.Destination })
                  .ToList()
                  .ForEach(s => shortestRoutes.Set(s, Infinity, null));
        }

        private TNode GetLocationToProcess(Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> shortestRoutes, List<TNode> locationsProcessed)
        {
            var unprocessedShortestRouteOrigin = GetUnprocessedShortestRoutesOrigins(shortestRoutes, locationsProcessed).First();
            var shortestRouteCost = shortestRoutes[unprocessedShortestRouteOrigin].Key;
            if (shortestRouteCost == Infinity)
                return default(TNode);

            return unprocessedShortestRouteOrigin; 
        }

        private static int Infinity
        {
            get { return Int32.MaxValue; }
        }

        private IEnumerable<TNode> GetUnprocessedShortestRoutesOrigins(Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> shortestRoutes, List<TNode> locationsProcessed)
        {
            return GetShortestRoutesOriginLocation(shortestRoutes).Where(location => !locationsProcessed.Contains(location));
        }

        private IEnumerable<TNode> GetShortestRoutesOriginLocation(Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> shortestRoutes)
        {
            return shortestRoutes.OrderBy(p => p.Value.Key)
                                 .Select(p => p.Key).ToList();
        }

        public async Task<List<IJourney<TNode>>> BreadthFirstSearchRoutesWithPortRepetitionLambdaAsync(TNode start, TNode destination,
            int maxNumberOfStops, Func<int, IJourney<TNode>, bool> journeyComparer)
        {
            var resultRoutes = new List<IJourney<TNode>>();
            IJourney<TNode> journey = new Journey<TNode>().WithPort(start);

            var queue = new Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>>();
            queue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(journey, new Route<TNode>(default(TNode), start, 0)));
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
                await ProcessAdjacentRoutesAsync(destination, currentNode, resultRoutes, queue);
            }
            return resultRoutes;
        }

        private async Task ProcessAdjacentRoutesAsync(TNode destination, KeyValuePair<IJourney<TNode>, IRoute<TNode>> currentNode,
           List<IJourney<TNode>> resultRoutes, Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>> queue)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination);
            foreach (var route in adjacentRoutes)
            {
                var nextjourney = GetNextJourney(currentNode, route);

                await ProcessJourneyAsync(destination, route, resultRoutes, nextjourney, queue);
            }
        }

        private Task ProcessJourneyAsync(TNode destination, IRoute<TNode> route, List<IJourney<TNode>> resultRoutes, IJourney<TNode> nextjourney, Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>> queue)
        {
            if (route.Destination.Equals(destination))
            {
                resultRoutes.Add(nextjourney);                
            }
            else
            {
                queue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(nextjourney, route));
            }
            return Task.FromResult(resultRoutes);
        }
    }

    public static class ExtensionMethod
    {
        public static void Set<TNode>(this Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> dictionary, TNode destination, int cost, IJourney<TNode> journey)
        {
            var completeRoute = journey ?? new Journey<TNode>();
            dictionary[destination] = new KeyValuePair<int, IJourney<TNode>>(cost, completeRoute);
        }
    }
}
