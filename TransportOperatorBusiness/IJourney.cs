using System;
using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IJourney<TNode> : ICloneable
    {
        Journey<TNode> WithPort(TNode port);
        int GetTime(IRouteRepository<IPort> routeRepository);
        bool IsValid(IRouteRepository<IPort> routeRepository);
        int NumberOfStops();
        List<TNode> Ports { get; }
    }
}