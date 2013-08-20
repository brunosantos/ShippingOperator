using System.Collections.Generic;

namespace TransportOperatorBusiness.Repositories
{
    public interface IPortRepository<TNode>
    {
        List<TNode> GetAllPorts();
        TNode GetPort(string portName);
    }
}