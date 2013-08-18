﻿using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    public class TransportOperator
    {
        private readonly IRouteRepository<IPort> _routeRepository;
        private readonly IPortRepository<IPort> _portRepository;
        private Graph _graph;

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
            _graph = new Graph(routeRepository);
        }

        public int GetNumberOfRoutesBetweenPortsWithNumberOfStops(IPort source, IPort destination, int numberOfStops)
        {
            var result = _graph.BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, numberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);

            return result.Count(x => x.NumberOfStops().Equals(numberOfStops));
        }

        public int GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(IPort source, IPort destination, int maxNumberOfStops)
        {
            List<IJourney<IPort>> result = _graph.BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, maxNumberOfStops,
                (numberOfNodes, journey) => journey.NumberOfStops() >= numberOfNodes);
            return result.Count(x => x.NumberOfStops() <= maxNumberOfStops);
        }

        public int GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(IPort source, IPort destination, int maxJourneytime)
        {
            var bfsRoutes = _graph.BreadthFirstSearchRoutesWithPortRepetitionLambda(source, destination, maxJourneytime,
                (mTime, journey) => journey.GetTime(_routeRepository) >= mTime);
            return bfsRoutes.Count();
        }

        public IJourney<IPort> GetShortestRoute(IPort source, IPort destination)
        {
            return _graph.GetShortestRoute(source, destination);
        }
    }
}