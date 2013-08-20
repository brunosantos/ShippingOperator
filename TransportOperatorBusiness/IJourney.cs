using System;
using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IJourney<TNode> : ICloneable
    {
        IJourney<TNode> WithPort(TNode port);
        int GetTime();
        bool IsValid();
        int NumberOfStops();
        List<TNode> Ports { get; }
    }
}