using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public class PortRepository : IPortRepository
    {
        private readonly List<IPort> _ports;

        public PortRepository()
        {
            _ports = new List<IPort>()
                         {
                             new Port("New York"),
                             new Port("Liverpool"),
                             new Port("Casablanca"),
                             new Port("Buenos Aires"),
                             new Port("Cape Town")
                         };
        }

        public List<IPort> GetAllPorts()
        {
            return _ports;
        }

        public IPort GetPort(string portName)
        {
            return _ports.Find(x => x.Name == portName);
        }
    }
}