using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    public class TransportOperator
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IPortRepository _portRepository;
        public List<IRoute<IPort>> Routes
        {
            get { return _routeRepository.GetAllRoutes(); }
        }
        public List<IPort> Ports {
            get { return _portRepository.GetAllPorts(); }
        }            

        public TransportOperator(IRouteRepository routeRepository, IPortRepository portRepository)
        {
            _routeRepository = routeRepository;
            _portRepository = portRepository;
        }

        public int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, int numberOfStops)
        {
            var result = Dijkstra.BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, Routes, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, int maxNumberOfStops)
        {
            List<IJourney<IPort>> result = Dijkstra.BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, Routes, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(IPort source, IPort destination, int maxJourneytime)
        {
            var bfsRoutes = Dijkstra.BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, Routes, maxJourneytime,
                (mTime, journey) => journey.GetTime(_routeRepository) >= mTime);
            return bfsRoutes.Count();
        }
    }
}