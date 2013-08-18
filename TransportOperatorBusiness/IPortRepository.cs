using System.Collections.Generic;

namespace TransportOperatorBusiness
{
    public interface IPortRepository
    {
        List<IPort> GetAllPorts();
        IPort GetPort(string portName);
    }
}