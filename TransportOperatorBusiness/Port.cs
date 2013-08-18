namespace TransportOperatorBusiness
{
    public interface IPort
    {
        string Name { get; }
    }

    public struct Port : IPort
    {
        private readonly string _name;

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