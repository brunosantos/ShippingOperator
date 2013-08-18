namespace TransportOperatorBusiness
{
    public interface IRoute
    {
        IPort Origin { get; }
        IPort Destination { get; }
        int RouteTimeInDays { get; }
    }
}