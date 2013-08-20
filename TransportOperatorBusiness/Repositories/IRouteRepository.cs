using System.Collections.Generic;

namespace TransportOperatorBusiness.Repositories
{
    public interface IRouteRepository<TNode>
    {
        List<IRoute<TNode>> GetAllRoutes();
        bool IsValidRoute(TNode portOrigin, TNode portDestination);
        int GetRouteTime(TNode portOrigin, TNode portDestination);
    }
}