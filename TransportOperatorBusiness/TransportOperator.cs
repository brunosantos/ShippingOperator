using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportOperatorBusiness
{
    public class TransportOperator
    {
        private readonly IRouteRepository<IPort> _routeRepository;
        private readonly IPortRepository<IPort> _portRepository;
        private Graph<IPort> _graph;

        public List<IRoute<IPort>> Routes
        {
            get { return _routeRepository.GetAllRoutes(); }
        }
        public List<IPort> Ports {
            get { return _portRepository.GetAllPorts(); }
        }

        public TransportOperator(IRouteRepository<IPort> routeRepository, IPortRepository<IPort> portRepository)
        {
            _routeRepository = routeRepository;
            _portRepository = portRepository;
            _graph = new Graph<IPort>(routeRepository);
        }

        public int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, int numberOfStops)
        {
            var result = _graph.BreadthFirstSearchRoutesWithPortRepetition(source, destination, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, int maxNumberOfStops)
        {
            List<IJourney<IPort>> result = _graph.BreadthFirstSearchRoutesWithPortRepetition(source, destination, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(IPort source, IPort destination, int maxJourneytime)
        {
            var bfsRoutes = _graph.BreadthFirstSearchRoutesWithPortRepetition(source, destination, maxJourneytime,
                (mTime, journey) => journey.GetTime() >= mTime);
            return bfsRoutes.Count();
        }

        public IJourney<IPort> GetShortestRoute(IPort source, IPort destination)
        {
            return _graph.GetShortestRoute(source, destination);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTimeAsync(IPort source, IPort destination, int maxNumberOfStops)
        {            
            Task<List<IJourney<IPort>>> breadthFirstSearchRoutesWithPortRepetitionLambdaAsyncResult = _graph.BreadthFirstSearchRoutesWithPortRepetitionAsync(source, destination, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            List<IJourney<IPort>> result = breadthFirstSearchRoutesWithPortRepetitionLambdaAsyncResult.Result;
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }
    }
}