﻿using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IRouteRepository
    {
        List<IRoute> GetAllRoutes();
        bool IsValidRoute(IPort portOrigin, IPort portDestination);
        int GetRouteTime(IPort portOrigin, IPort portDestination);
    }

    public class RouteRepository : IRouteRepository
    {
        private readonly IPortRepository _portRepository;
        private readonly List<IRoute> _routes;
        public RouteRepository(IPortRepository portRepository)
        {
            _portRepository = portRepository;
            _routes = new List<IRoute>
                {
                    new Route(GetPort("Buenos Aires"), GetPort("New York"), 6),
                    new Route(GetPort("Buenos Aires"), GetPort("Casablanca"), 5),
                    new Route(GetPort("Buenos Aires"), GetPort("Cape Town"), 4),
                    new Route(GetPort("New York"), GetPort("Liverpool"), 4),
                    new Route(GetPort("Liverpool"), GetPort("Casablanca"), 3),
                    new Route(GetPort("Liverpool"), GetPort("Cape Town"), 6),
                    new Route(GetPort("Casablanca"), GetPort("Liverpool"), 3),
                    new Route(GetPort("Casablanca"), GetPort("Cape Town"), 6),
                    new Route(GetPort("Cape Town"), GetPort("New York"), 8)
                };
        }

        protected List<IRoute> Routes
        {
            get { return _routes; }            
        }

        public List<IRoute> GetAllRoutes()
        {
            return _routes;
        }

        public bool IsValidRoute(IPort portOrigin, IPort portDestination)
        {
            return _routes.Exists(x => x.Origin == portOrigin && x.Destination == portDestination);
        }

        public int GetRouteTime(IPort portOrigin, IPort portDestination)
        {
            return _routes.Find(x => x.Origin == portOrigin && x.Destination == portDestination).RouteTimeInDays;
        }

        private IPort GetPort(string portName)
        {
            return _portRepository.GetPort(portName);
        }
    }
}