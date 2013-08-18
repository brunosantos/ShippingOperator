using NUnit.Framework;
using TransportOperatorBusiness;

namespace UnitTests
{
    [TestFixture]
    public class JourneyTests
    {
        private TransportOperator _transportOperator;
        //private readonly RouteTests _routeTests = new RouteTests();
        private IPortRepository _portRepository;
        private IRouteRepository _routeRepository;

        [SetUp]
        public void Setup()
        {
            _portRepository = new PortRepository();
            _routeRepository = new RouteRepository(_portRepository);
            _transportOperator = new TransportOperator(_routeRepository, _portRepository);

        }

        [Test]
        public void ShouldDetectInvalidJourney()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCapetown = _portRepository.GetPort("Cape Town");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");

            var invalidJourney = new Journey<IPort>().WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portCasablanca);
            var validJourney = new Journey<IPort>().WithPort(portBuenosAires).WithPort(portNy).WithPort(portLiverpool);

            Assert.IsFalse(invalidJourney.IsValid(_routeRepository));
            Assert.IsTrue(validJourney.IsValid(_routeRepository));
        }

        [Test]
        public void ShouldGetAJourneyTime()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var journey = new Journey<IPort>().WithPort(portBuenosAires).WithPort(portNy).WithPort(portLiverpool);
            //var invalidjourney = new Journey(_routeRepository).WithPort(portBuenosAires).WithPort(portNy).WithPort(portCapetown);

            var journey2 = new Journey<IPort>().WithPort(portBuenosAires).WithPort(portCasablanca).WithPort(portLiverpool);
            var journey3 = new Journey<IPort>().WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portNy).WithPort(portLiverpool).WithPort(portCasablanca);
            var invalidjourney = new Journey<IPort>().WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portCasablanca);

            
            Assert.That(journey.GetTime(_routeRepository), Is.EqualTo(10));
            Assert.That(journey2.GetTime(_routeRepository), Is.EqualTo(8));
            Assert.That(journey3.GetTime(_routeRepository), Is.EqualTo(19));
            Assert.That(invalidjourney.GetTime(_routeRepository), Is.EqualTo(0));
        }
    }
}
