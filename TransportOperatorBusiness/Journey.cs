using System;
using System.Collections.Generic;
using System.Linq;

namespace TransportOperatorBusiness
{
    public interface IJourney : ICloneable
    {
        Journey WithPort(IPort port);
        int GetTime(IRouteRepository routeRepository);
        bool IsValid(IRouteRepository routeRepository);
        int NumberOfStops();
    }

    //maybe the journey should have routes instead of ports. and should throw exception if route added was invalid?
    //it would be easier to get totaltime because we wouldn't have to call repository for that...
    public class Journey : IJourney
    {
        private List<IPort> _ports;

        public Journey()
        {
            _ports = new List<IPort>();
        }
        public Journey(IRoute<IPort> route)
        {
            _ports = new List<IPort>() { route.Origin, route.Destination};            
        }

        public Journey WithPort(IPort port)
        {
            _ports.Add(port);
            return this;
        }

        public int GetTime(IRouteRepository routeRepository)
        {
            //assume invalid journey time is 0
            int tcount = 0;
            if (IsValid(routeRepository))
            {
                //TODO Refactor This -should I look twice?!?! what about single responsability?
                for (int i = 0; i < _ports.Count - 1; ++i)
                {
                    tcount += GetTime(_ports[i], _ports[i + 1], routeRepository);
                }
            }
            return tcount;
        }

        private int GetTime(IPort portOrigin, IPort portDestination, IRouteRepository routeRepository)
        {
            return routeRepository.GetRouteTime(portOrigin, portDestination);
        }

        public bool IsValid(IRouteRepository routeRepository)
        {
            if (HasMoreThanTwoPorts())
            {
                //TODO Refactor This
                for (int i = 0; i < _ports.Count - 1; ++i)
                {
                    if (!IsValid(_ports[i], _ports[i + 1], routeRepository))
                        return false;
                }
            }

            //is a journey with only one port valid?
            return true;
        }

        public int NumberOfStops()
        {
            return _ports.Count == 0 ? 0 : _ports.Count-1;
        }

        private bool HasMoreThanTwoPorts()
        {
            return _ports.Count >= 2;
        }

        private bool IsValid(IPort portOrigin, IPort portDestination, IRouteRepository routeRepository)
        {
            //can whe find a route that matches this?
            //it would be nice if I could just create a route and do contains on routes...
            return routeRepository.IsValidRoute(portOrigin, portDestination);
        }

        public object Clone()
        {
            var clone = new Journey {_ports = new List<IPort>()};
            foreach (var port in _ports)
                clone._ports.Add(port);
            
            return clone;
        }
    }
}