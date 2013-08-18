using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public class TransportOperatorBuilder
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IPortRepository _portRepository;
        public List<IRoute> Routes {
            get { return _routeRepository.GetAllRoutes(); }
        }
        public List<IPort> Ports {
            get { return _portRepository.GetAllPorts(); }
        }            

        public TransportOperatorBuilder(IRouteRepository routeRepository, IPortRepository portRepository)
        {
            _routeRepository = routeRepository;
            _portRepository = portRepository;
        }

        //public TransportOperatorBuilder WithRoute(IRoute route)
        //{
        //    Routes.Add(route);
        //    return this;
        //}

    }
}