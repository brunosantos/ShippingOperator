using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TransportOperatorBusiness;

namespace UnitTests
{
    [TestFixture]
    public class Tests
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
            _transportOperator = new TransportOperator(_routeRepository,_portRepository);
            
        }

        [Test]
        public void ShouldHaveARouteWithASingleDirectionFromNyToLiverpool()
        {
            IPort portA = _portRepository.GetPort("New York");
            IPort portB = _portRepository.GetPort("Liverpool");

            IRoute<IPort> expectedRoute = new Route<IPort>(portA, portB, 4);
            IRoute<IPort> unexpectedRoute = new Route<IPort>(portB, portA, 4);

            Assert.IsTrue(_transportOperator.Routes.Contains(expectedRoute));
            Assert.IsFalse(_transportOperator.Routes.Contains(unexpectedRoute));            
        }

        [Test]
        public void ShouldHaveARouteWithDirectReturnFromLiverpoolToCasablanca()
        {
            IPort portA = _portRepository.GetPort("Liverpool");
            IPort portB = _portRepository.GetPort("Casablanca");
            IRoute<IPort> expectedRoute = new Route<IPort>(portA, portB, 3);
            IRoute<IPort> expectedRouteReturn = new Route<IPort>(portB, portA, 3);

            Assert.IsTrue(_transportOperator.Routes.Contains(expectedRoute));
            Assert.IsTrue(_transportOperator.Routes.Contains(expectedRouteReturn));      
        }      

        //[Test]
        //public void ShouldGetShortestJourneyFromBuenosAiresToLiverpool()
        //{
        //    IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
        //    IPort portLiverpool = _portRepository.GetPort("Liverpool");
        //    IPort portCasablanca = _portRepository.GetPort("Casablanca");

        //    var results = Graph.GetShortestRoute(portBuenosAires, portLiverpool,
        //                                                          _routeRepository.GetAllRoutes());

        //    Assert.That(results.Sum(r => r.RouteTimeInDays), Is.EqualTo(8));
        //    Assert.That(results.Count, Is.EqualTo(2));
        //    Assert.That(results.First().RouteTimeInDays, Is.EqualTo(5));
        //    Assert.That(results.First().Origin, Is.EqualTo(portBuenosAires));
        //    Assert.That(results.First().Destination, Is.EqualTo(portCasablanca));
        //    Assert.That(results.Skip(1).First().RouteTimeInDays, Is.EqualTo(3));
        //    Assert.That(results.Skip(1).First().Origin, Is.EqualTo(portCasablanca));
        //    Assert.That(results.Skip(1).First().Destination, Is.EqualTo(portLiverpool)); 
        //}

        [Test]
        public void ShouldGetShortestJourneyFromNyToNy()
        {
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var results = _transportOperator.GetShortestRoute(portNy, portNy);

            Assert.That(results.GetTime(_routeRepository), Is.EqualTo(18));
            Assert.That(results.NumberOfStops(), Is.EqualTo(3));

            Assert.That(results.Ports.First(), Is.EqualTo(portNy));
            Assert.That(results.Ports.Skip(1).First(), Is.EqualTo(portLiverpool));
            Assert.That(results.Ports.Skip(2).First(), Is.EqualTo(portCapetown));
            Assert.That(results.Ports.Skip(3).First(), Is.EqualTo(portNy));
        }


        //[Test]
        //public void ShouldGetShortestJourneyFromLiverpoolToLiverpool()
        //{
        //    IPort portLiverpool = _portRepository.GetPort("Liverpool");
        //    IPort portCasablanca = _portRepository.GetPort("Casablanca");

        //    var results = Graph.GetShortestRoute(portLiverpool, portLiverpool, _routeRepository.GetAllRoutes());

        //    Assert.That(results.Sum(r => r.RouteTimeInDays), Is.EqualTo(6));
        //    Assert.That(results.Count, Is.EqualTo(2));
        //    Assert.That(results.First().RouteTimeInDays, Is.EqualTo(3));
        //    Assert.That(results.First().Origin, Is.EqualTo(portLiverpool));
        //    Assert.That(results.First().Destination, Is.EqualTo(portCasablanca));
        //    Assert.That(results.Skip(1).First().RouteTimeInDays, Is.EqualTo(3));
        //    Assert.That(results.Skip(1).First().Origin, Is.EqualTo(portCasablanca));
        //    Assert.That(results.Skip(1).First().Destination, Is.EqualTo(portLiverpool));
        //}



        [Test]
        public void ShouldGetShortestJourneyFromLiverpoolToLiverpool()
        {
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");

            var results = _transportOperator.GetShortestRoute(portLiverpool, portLiverpool);

            Assert.That(results.GetTime(_routeRepository), Is.EqualTo(6));
            Assert.That(results.NumberOfStops(), Is.EqualTo(2));
            
            Assert.That(results.Ports.First(), Is.EqualTo(portLiverpool));
            Assert.That(results.Ports.Skip(1).First(), Is.EqualTo(portCasablanca));
            Assert.That(results.Ports.Skip(2).First(), Is.EqualTo(portLiverpool));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithMaxNumberOfThreeStops()
        {
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var numberOfRoutes = _transportOperator.GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(portLiverpool, portLiverpool, 3);
            Assert.That(numberOfRoutes, Is.EqualTo(2));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromBuenosAiresToLiverpoolWithFourStops()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var results = _transportOperator.GetNumberOfRoutesBetweenPortsWithNumberOfStops(portBuenosAires, portLiverpool,  4);

            Assert.That(results, Is.EqualTo(1));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithJourneyTimeEqualOrLessThanTwentyFive()
        {
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var results = _transportOperator.GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(portLiverpool, portLiverpool, 25);

            Assert.That(results, Is.EqualTo(3));
        }      
    }
}
