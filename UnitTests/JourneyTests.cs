using System.Collections.Generic;
using NUnit.Framework;
using TransportOperatorBusiness;

namespace UnitTests
{
    [TestFixture]
    public class JourneyTests
    {
        private TransportOperator _transportOperator;
        private IPortRepository<IPort> _portRepository;
        private IRouteRepository<IPort> _routeRepository;

        [SetUp]
        public void Setup()
        {
            var ports = new List<IPort>()
                         {
                             new Port("New York"),
                             new Port("Liverpool"),
                             new Port("Casablanca"),
                             new Port("Buenos Aires"),
                             new Port("Cape Town")
                         };

            _portRepository = new PortRepository<IPort>(ports);
            _routeRepository = new RouteRepository<IPort>(_portRepository);
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

            var invalidJourney = new Journey<IPort>(_routeRepository).WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portCasablanca);
            var validJourney = new Journey<IPort>(_routeRepository).WithPort(portBuenosAires).WithPort(portNy).WithPort(portLiverpool);

            Assert.IsFalse(invalidJourney.IsValid());
            Assert.IsTrue(validJourney.IsValid());
        }

        [Test]
        public void ShouldGetAJourneyTime()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var journey = new Journey<IPort>(_routeRepository).WithPort(portBuenosAires).WithPort(portNy).WithPort(portLiverpool);
            var journey2 = new Journey<IPort>(_routeRepository).WithPort(portBuenosAires).WithPort(portCasablanca).WithPort(portLiverpool);
            var journey3 = new Journey<IPort>(_routeRepository).WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portNy).WithPort(portLiverpool).WithPort(portCasablanca);
            var invalidjourney = new Journey<IPort>(_routeRepository).WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portCasablanca);

            
            Assert.That(journey.GetTime(), Is.EqualTo(10));
            Assert.That(journey2.GetTime(), Is.EqualTo(8));
            Assert.That(journey3.GetTime(), Is.EqualTo(19));
            Assert.That(invalidjourney.GetTime(), Is.EqualTo(0));
        }
    }
}
