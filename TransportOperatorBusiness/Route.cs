using System;

namespace TransportOperatorBusiness
{
    public struct Route<TNode> : IRoute<TNode>
    {
        private readonly TNode _origin;
        private readonly TNode _destination;
        private readonly int _routeTimeInDays;

        public Route(TNode origin, TNode destination, int routeTimeInDays)
        {
            _origin = origin;
            _destination = destination;
            _routeTimeInDays = routeTimeInDays;
        }

        public TNode Origin
        {
            get { return _origin; }
        }

        public TNode Destination
        {
            get { return _destination; }
        }

        public int RouteTimeInDays
        {
            get { return _routeTimeInDays; }
        }
    }    
}