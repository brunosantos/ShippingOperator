using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IRouteRepository<TNode>
    {
        List<IRoute<TNode>> GetAllRoutes();
        bool IsValidRoute(TNode portOrigin, TNode portDestination);
        int GetRouteTime(TNode portOrigin, TNode portDestination);
    }

    public class RouteRepository<TNode> : IRouteRepository<TNode>
    {
        private readonly IPortRepository<IPort> _portRepository;
        private readonly List<IRoute<TNode>> _routes;
        public RouteRepository(IPortRepository<IPort> portRepository)
        {
            _portRepository = portRepository;
            _routes = new List<IRoute<TNode>>
                {
                    new Route<TNode>(GetPort("Buenos Aires"), GetPort("New York"), 6),
                    new Route<TNode>(GetPort("Buenos Aires"), GetPort("Casablanca"), 5),
                    new Route<TNode>(GetPort("Buenos Aires"), GetPort("Cape Town"), 4),
                    new Route<TNode>(GetPort("New York"), GetPort("Liverpool"), 4),
                    new Route<TNode>(GetPort("Liverpool"), GetPort("Casablanca"), 3),
                    new Route<TNode>(GetPort("Liverpool"), GetPort("Cape Town"), 6),
                    new Route<TNode>(GetPort("Casablanca"), GetPort("Liverpool"), 3),
                    new Route<TNode>(GetPort("Casablanca"), GetPort("Cape Town"), 6),
                    new Route<TNode>(GetPort("Cape Town"), GetPort("New York"), 8)
                };
        }

        protected List<IRoute<TNode>> Routes
        {
            get { return _routes; }            
        }

        public List<IRoute<TNode>> GetAllRoutes()
        {
            return _routes;
        }

        public bool IsValidRoute(TNode portOrigin, TNode portDestination)
        {
            return _routes.Exists(x => x.Origin.Equals(portOrigin) && x.Destination.Equals(portDestination));
        }

        public int GetRouteTime(TNode portOrigin, TNode portDestination)
        {
            return _routes.Find(x => x.Origin.Equals(portOrigin) && x.Destination.Equals(portDestination)).RouteTimeInDays;
        }

        private TNode GetPort(string portName)
        {
            return (TNode) _portRepository.GetPort(portName);
        }
    }
}