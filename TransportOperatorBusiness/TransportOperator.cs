using System.Collections.Generic;

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

    }
}