using System;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    //maybe the journey should have routes instead of ports. and should throw exception if route added was invalid?
    //it would be easier to get totaltime because we wouldn't have to call repository for that...
    public class Journey<TNode> : IJourney<TNode>
    {
        private List<TNode> _ports;

        public Journey()
        {
            _ports = new List<TNode>();
        }
        public Journey(IRoute<TNode> route)
        {
            _ports = new List<TNode>() { route.Origin, route.Destination };            
        }

        public Journey(IEnumerable<IRoute<TNode>> route)
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

        public int GetTime(IRouteRepository<TNode> routeRepository)
        {
            //assume invalid journey time is 0
            int tcount = 0;
            if (IsValid(routeRepository))
            {
                //TODO Refactor This -should I look twice?!?! what about single responsibility?
                for (int i = 0; i < Ports.Count - 1; ++i)
                {
                    tcount += GetTime(Ports[i], Ports[i + 1], routeRepository);
                }
            }
            return tcount;
        }

        private int GetTime(TNode portOrigin, TNode portDestination, IRouteRepository<TNode> routeRepository)
        {
            return routeRepository.GetRouteTime(portOrigin, portDestination);
        }

        public bool IsValid(IRouteRepository<TNode> routeRepository)
        {
            if (HasMoreThanTwoPorts())
            {
                //TODO Refactor This
                for (int i = 0; i < Ports.Count - 1; ++i)
                {
                    if (!IsValid(Ports[i], Ports[i + 1], routeRepository))
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

        private bool IsValid(TNode portOrigin, TNode portDestination, IRouteRepository<TNode> routeRepository)
        {
            //can whe find a route that matches this?
            //it would be nice if I could just create a route and do contains on routes...
            return routeRepository.IsValidRoute(portOrigin, portDestination);
        }

        public object Clone()
        {
            var clone = new Journey<TNode> { _ports = new List<TNode>() };
            foreach (var port in Ports)
                clone.Ports.Add(port);
            
            return clone;
        }
    }
}