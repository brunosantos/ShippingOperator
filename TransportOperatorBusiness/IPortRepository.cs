using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IPortRepository<TNode>
    {
        List<TNode> GetAllPorts();
        TNode GetPort(string portName);
    }
}