namespace TransportOperatorBusiness
{
    public interface IRoute<TNode>
    {
        TNode Origin { get; }
        TNode Destination { get; }
        int RouteTimeInDays { get; }
    }
}