using System;
using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IJourney<TNode> : ICloneable
    {
        Journey<TNode> WithPort(TNode port);
        int GetTime(IRouteRepository<TNode> routeRepository);
        bool IsValid(IRouteRepository<TNode> routeRepository);
        int NumberOfStops();
        List<TNode> Ports { get; }
    }
}