using System;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    public class Journey<TNode> : IJourney<TNode>
    {
        private readonly IRouteRepository<TNode> _routeRepository;
        private List<TNode> _ports = new List<TNode>();

        public Journey(IRouteRepository<TNode> routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public Journey(IRoute<TNode> route, IRouteRepository<TNode> routeRepository)
            : this(routeRepository)
        {
            _ports = new List<TNode>() { route.Origin, route.Destination };            
        }

        public Journey(IEnumerable<IRoute<TNode>> route, IRouteRepository<TNode> routeRepository)
            : this(routeRepository)
        {
            _ports = route.Select(x => x.Origin).ToList();
        }

        public List<TNode> Ports
        {
            get { return _ports; }
        }

        public IJourney<TNode> WithPort(TNode port)
        {
            Ports.Add(port);
            return this;
        }

        public int GetTime()
        {
            //assume invalid journey time is 0
            int tcount = 0;
            if (IsValid())
            {
                //TODO Refactor This -should I look twice?!?! what about single responsibility?
                for (int i = 0; i < Ports.Count - 1; ++i)
                {
                    tcount += _routeRepository.GetRouteTime(Ports[i], Ports[i + 1]);
                }
            }
            return tcount;
        }

        public bool IsValid()
        {
            if (HasMoreThanTwoPorts())
            {
                //TODO Refactor This
                for (int i = 0; i < Ports.Count - 1; ++i)
                {
                    if (!IsValid(Ports[i], Ports[i + 1]))
                        return false;
                }
            }

            //is a journey with only one port valid?
            return true;
        }

        public int NumberOfStops()
        {
            return Ports.Count == 0 ? 0 : Ports.Count-1;
        }

        private bool HasMoreThanTwoPorts()
        {
            return Ports.Count >= 2;
        }

        private bool IsValid(TNode portOrigin, TNode portDestination)
        {
            //can whe find a route that matches this?
            //it would be nice if I could just create a route and do contains on routes...
            return _routeRepository.IsValidRoute(portOrigin, portDestination);
        }

        public object Clone()
        {
            var clone = new Journey<TNode>(_routeRepository) { _ports = new List<TNode>() };
            foreach (var port in Ports)
                clone.Ports.Add(port);
            
            return clone;
        }
    }
}