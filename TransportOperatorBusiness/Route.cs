using System;

namespace TransportOperatorBusiness
{
    public class Route : IRoute, IEquatable<Route>
    {       
        private readonly IPort _origin;
        private readonly IPort _destination;
        private readonly int _routeTimeInDays;

        public Route(IPort origin, IPort destination, int routeTimeInDays)
        {
            _origin = origin;
            _destination = destination;
            _routeTimeInDays = routeTimeInDays;
        }

        public IPort Origin
        {
            get { return _origin; }
        }

        public IPort Destination
        {
            get { return _destination; }
        }

        public int RouteTimeInDays
        {
            get { return _routeTimeInDays; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Route) obj);
        }

        public bool Equals(Route other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_origin, other._origin) && Equals(_destination, other._destination) && _routeTimeInDays == other._routeTimeInDays;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_origin != null ? _origin.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_destination != null ? _destination.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _routeTimeInDays;
                return hashCode;
            }
        }

        public static bool operator ==(Route left, Route right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Route left, Route right)
        {
            return !Equals(left, right);
        }

    }    
}