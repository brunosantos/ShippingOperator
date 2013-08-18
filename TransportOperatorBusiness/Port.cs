namespace TransportOperatorBusiness
{
    public interface IPort
    {
        string Name { get; }
    }

    public class Port : IPort
    {
        private string _name;

        public Port(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}