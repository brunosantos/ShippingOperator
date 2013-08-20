using System.Collections.Generic;

namespace TransportOperatorBusiness.Repositories
{
    public class PortRepository<TNode> : IPortRepository<TNode>
    {
        private readonly List<TNode> _ports;

        public PortRepository(List<TNode> ports)
        {
            _ports = ports;
        }

        public List<TNode> GetAllPorts()
        {
            return _ports;
        }

        public TNode GetPort(string portName)
        {
            return _ports.Find(x => x.GetType().GetProperty("Name").GetValue(x).Equals(portName));
        }
    }
}