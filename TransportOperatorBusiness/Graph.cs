using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TransportOperatorBusiness
{
    public class Graph<TNode>
    {
        private readonly IRouteRepository<TNode> _routeRepository;
        private readonly List<IRoute<TNode>> _routes;

        public Graph(IRouteRepository<TNode> routeRepository)
        {            
            _routeRepository = routeRepository;            
            _routes = _routeRepository.GetAllRoutes();

        }

        public int GetNumberOfRoutesBetweenPortsWithNumberOfStops(TNode source, TNode destination, int numberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetition(source, destination, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(TNode source, TNode destination, int maxNumberOfStops)
        {
            var result = BreadthFirstSearchRoutesWithPortRepetition(source, destination, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(TNode source, TNode destination, int maxJourneytime)
        {
            var bfsRoutes = BreadthFirstSearchRoutesWithPortRepetition(source, destination, maxJourneytime,
                (mTime, journey) => journey.GetTime() >= mTime);
            return bfsRoutes.Count();
        }

        private IEnumerable<IRoute<TNode>> GetAdjacentRoutes(TNode port)
        {
            //Task.Delay(500);
            var x =_routes.Where(n => n.Origin.Equals(port));
            return x;
        }

        public List<IJourney<TNode>> BreadthFirstSearchRoutesWithPortRepetition(TNode start, TNode destination,
            int maxNumberOfStops, Func<int, IJourney<TNode>, bool> journeyComparer)
        {
            IJourney<TNode> journey = new Journey<TNode>(_routeRepository).WithPort(start);
                 
            var status = new BreadthFirstSearchRoutesStatus<TNode>
                             {
                                 NodeToProcessQueue = new Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>>(), 
                                 ResultJourneys = new List<IJourney<TNode>>()
                             };

            status.NodeToProcessQueue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(journey, new Route<TNode>(default(TNode), start, 0)));

            while (status.NodeToProcessQueue.Count != 0)
            {
                var currentNode = status.NodeToProcessQueue.Dequeue();

                //could this scope be an async call !?!?
                if (maxNumberOfStops == 0 && status.NodeToProcessQueue.Count == 0)
                {
                    return status.ResultJourneys;
                }

                var currentjourney = currentNode.Key;
                if (journeyComparer(maxNumberOfStops, currentjourney))
                    break;

                //this can be an async call.
                status = ProcessAdjacentRoutes(destination, currentNode, status);
            }
            return status.ResultJourneys;
        }

        private BreadthFirstSearchRoutesStatus<TNode> ProcessAdjacentRoutes(TNode destination, KeyValuePair<IJourney<TNode>, IRoute<TNode>> currentNode,
            BreadthFirstSearchRoutesStatus<TNode> status)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination);            
            foreach (var route in adjacentRoutes)
            {
                var nextjourney = GetNextJourney(currentNode, route);
                status=ProcessRoute(destination, route, nextjourney, status);
            }
            return status;
        }

        private BreadthFirstSearchRoutesStatus<TNode> ProcessRoute(TNode destination, IRoute<TNode> route, IJourney<TNode> nextjourney, BreadthFirstSearchRoutesStatus<TNode> status)
        {
            //for testing purposes.
            //to prove that async is faster only if the proccess is slower.
            //Task.Delay(500);
            if (route.Destination.Equals(destination))
            {
                status.ResultJourneys.Add(nextjourney);
            }
            else
            {
                status.NodeToProcessQueue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(nextjourney, route));
            }

            return status;
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
            var adjacentNodes = GetAdjacentNodes(source);
            IJourney<TNode> shortestJourney = null;
            foreach (var node in adjacentNodes)
            {
                var currentRoute = routeDictionary[node];

                if ((currentRoute.NumberOfStops() > 0 && shortestJourney == null) ||
                    shortestJourney != null && shortestJourney.GetTime() > currentRoute.GetTime())
                {                    
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
            shortestRoutes.Set(source, 0, new Journey<TNode>(_routeRepository).WithPort(source), _routeRepository);

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
                        var journey = ((IJourney<TNode>)shortestRoutes[route.Origin].Value.Clone()).WithPort(route.Destination);
                        shortestRoutes.Set(route.Destination, route.RouteTimeInDays + shortestRoutes[route.Origin].Key, journey, _routeRepository);
                    }
                }
                locationsProcessed.Add(locationToProcess);
            }
            return shortestRoutes.ToDictionary(k => k.Key, v => v.Value.Value);
        }

        private void SetInfinityToAllRoutes(Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> shortestRoutes)
        {
            _routes.SelectMany(p => new TNode[] { p.Origin, p.Destination })
                  .ToList()
                  .ForEach(s => shortestRoutes.Set(s, Infinity, null, _routeRepository));
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

        public async Task<List<IJourney<TNode>>> BreadthFirstSearchRoutesWithPortRepetitionAsync(TNode start, TNode destination,
            int maxNumberOfStops, Func<int, IJourney<TNode>, bool> journeyComparer)
        {
            IJourney<TNode> journey = new Journey<TNode>(_routeRepository).WithPort(start);
                 
            var status = new BreadthFirstSearchRoutesStatus<TNode>
                             {
                                 NodeToProcessQueue = new Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>>(), 
                                 ResultJourneys = new List<IJourney<TNode>>()
                             };

            status.NodeToProcessQueue.Enqueue(new KeyValuePair<IJourney<TNode>, IRoute<TNode>>(journey, new Route<TNode>(default(TNode), start, 0)));
            
            while (status.NodeToProcessQueue.Count != 0)
            {
                var currentNode = status.NodeToProcessQueue.Dequeue();

                //could this scope be an async call !?!?
                if (maxNumberOfStops == 0 && status.NodeToProcessQueue.Count == 0)
                {
                    return status.ResultJourneys;
                }

                var currentjourney = currentNode.Key;
                if (journeyComparer(maxNumberOfStops, currentjourney))
                    break;

                status = await ProcessAdjacentRoutesAsync(destination, currentNode, status);                
            }
            return status.ResultJourneys;
        }

        private async Task<BreadthFirstSearchRoutesStatus<TNode>> ProcessAdjacentRoutesAsync(TNode destination, KeyValuePair<IJourney<TNode>, IRoute<TNode>> currentNode,
            BreadthFirstSearchRoutesStatus<TNode> status)
        {
            var adjacentRoutes = GetAdjacentRoutes(currentNode.Value.Destination);

            Parallel.ForEach(adjacentRoutes, route =>
            {
                var nextjourney = GetNextJourney(currentNode, route);
                status = ProcessRoute(destination, route, nextjourney, status);

            });

            return status;
        }
    }

    internal class BreadthFirstSearchRoutesStatus<TNode>
    {
        public Queue<KeyValuePair<IJourney<TNode>, IRoute<TNode>>> NodeToProcessQueue { get; set; }

        public List<IJourney<TNode>> ResultJourneys { get; set; }
    }

    internal static class ExtensionMethod
    {
        public static void Set<TNode>(this Dictionary<TNode, KeyValuePair<int, IJourney<TNode>>> dictionary, TNode destination, int cost, IJourney<TNode> journey, IRouteRepository<TNode> routeRepository)
        {
            var completeRoute = journey ?? new Journey<TNode>(routeRepository);
            dictionary[destination] = new KeyValuePair<int, IJourney<TNode>>(cost, completeRoute);
        }
    }
}
