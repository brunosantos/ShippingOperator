using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TransportOperatorBusiness;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        private TransportOperatorBuilder _transportOperatorBuilder;
        private IPortRepository _portRepository;
        private IRouteRepository _routeRepository;

        [SetUp]
        public void Setup()
        {
            _portRepository = new PortRepository();
            _routeRepository = new RouteRepository(_portRepository); 
            _transportOperatorBuilder = new TransportOperatorBuilder(_routeRepository,_portRepository);
            
        }

        [Test]
        public void ShouldHaveARouteWithASingleDirectionFromNyToLiverpool()
        {
            IPort portA = _portRepository.GetPort("New York");
            IPort portB = _portRepository.GetPort("Liverpool");
            
            IRoute expectedRoute = new Route(portA, portB, 4);
            IRoute unexpectedRoute = new Route(portB, portA, 4);

            Assert.IsTrue(_transportOperatorBuilder.Routes.Contains(expectedRoute));
            Assert.IsFalse(_transportOperatorBuilder.Routes.Contains(unexpectedRoute));            
        }

        [Test]
        public void ShouldOnlyHaveOneRouteWithTwoDirections()
        {
            IPort portA = _portRepository.GetPort("Liverpool");
            IPort portB = _portRepository.GetPort("Casablanca");
            IRoute expectedRoute = new Route(portA, portB, 3);

            Assert.IsTrue(_transportOperatorBuilder.Routes.Contains(expectedRoute));
            Assert.That(_transportOperatorBuilder.Routes.FindAll(x => x.GetType() == typeof (Route)).Count, Is.EqualTo(1));
        }      

        

        [Test]
        public void ShouldGetShortestJourneyFromBuenosAiresToLiverpool()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            //var expectedjourney = new Journey(_routeRepository).WithPort(portBuenosAires).WithPort(portNy).WithPort(portLiverpool);
            //var expectedjourney2 = new Journey(_routeRepository).WithPort(portBuenosAires).WithPort(portNy).WithPort(portLiverpool);

            //var journey2 = new Journey(_routeRepository).WithPort(portBuenosAires).WithPort(portCasablanca).WithPort(portLiverpool);
            //var journey3 = new Journey(_routeRepository).WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portNy).WithPort(portLiverpool).WithPort(portCasablanca);
            //var journey4 = new Journey(_routeRepository).WithPort(portBuenosAires).WithPort(portCapetown).WithPort(portCasablanca);


            var results = Dijkstra.GetShortestRoute(portBuenosAires, portLiverpool,
                                                                  _routeRepository.GetAllRoutes());


            Assert.That(results.Sum(r => r.RouteTimeInDays), Is.EqualTo(8));
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.First().RouteTimeInDays, Is.EqualTo(5));
            Assert.That(results.First().Origin, Is.EqualTo(portBuenosAires));
            Assert.That(results.First().Destination, Is.EqualTo(portCasablanca));
            Assert.That(results.Skip(1).First().RouteTimeInDays, Is.EqualTo(3));
            Assert.That(results.Skip(1).First().Origin, Is.EqualTo(portCasablanca));
            Assert.That(results.Skip(1).First().Destination, Is.EqualTo(portLiverpool)); 

        }

        [Test]
        public void ShouldGetShortestJourneyFromNyToNy()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var results = Dijkstra.GetShortestRoute(portNy, portNy, _routeRepository.GetAllRoutes());
            var results1 = Dijkstra.GetShortestRoute(portNy, portCapetown, _routeRepository.GetAllRoutes());
            var results2 = Dijkstra.GetShortestRoute(portNy, portBuenosAires, _routeRepository.GetAllRoutes());

            //need to create an adapted Dijkstra to ignore route from/to NY at RouteTimeInDays=0
            Assert.That(results.Sum(r => r.RouteTimeInDays), Is.EqualTo(18));
            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results.First().RouteTimeInDays, Is.EqualTo(4));
            Assert.That(results.First().Origin, Is.EqualTo(portNy));
            Assert.That(results.First().Destination, Is.EqualTo(portLiverpool));
            Assert.That(results.Skip(1).First().RouteTimeInDays, Is.EqualTo(6));
            Assert.That(results.Skip(1).First().Origin, Is.EqualTo(portLiverpool));
            Assert.That(results.Skip(1).First().Destination, Is.EqualTo(portCapetown));
            Assert.That(results.Skip(2).First().RouteTimeInDays, Is.EqualTo(8));
            Assert.That(results.Skip(2).First().Origin, Is.EqualTo(portCapetown));
            Assert.That(results.Skip(2).First().Destination, Is.EqualTo(portNy));
        }


        [Test]
        public void ShouldGetShortestJourneyFromLiverpoolToLiverpool()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var results = Dijkstra.GetShortestRoute(portLiverpool, portLiverpool, _routeRepository.GetAllRoutes());

            Assert.That(results.Sum(r => r.RouteTimeInDays), Is.EqualTo(6));
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.First().RouteTimeInDays, Is.EqualTo(3));
            Assert.That(results.First().Origin, Is.EqualTo(portLiverpool));
            Assert.That(results.First().Destination, Is.EqualTo(portCasablanca));
            Assert.That(results.Skip(1).First().RouteTimeInDays, Is.EqualTo(3));
            Assert.That(results.Skip(1).First().Origin, Is.EqualTo(portCasablanca));
            Assert.That(results.Skip(1).First().Destination, Is.EqualTo(portLiverpool));
            //Assert.That(results.Skip(2).First().RouteTimeInDays, Is.EqualTo(8));
            //Assert.That(results.Skip(2).First().Origin, Is.EqualTo(portCapetown));
            //Assert.That(results.Skip(2).First().Destination, Is.EqualTo(portNy));
        }

        [Test]
        public void ShouldGetAllRoutesFromBuenosAires()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var expectedLevel0 = new List<IRoute>() {
                new Route(portBuenosAires,portNy,6),
                new Route(portBuenosAires,portCasablanca,5),
                new Route(portBuenosAires,portCapetown,4)};

            var expectedLevel1 = new List<IRoute>() {
                new Route(portNy,portLiverpool,4),
                new Route(portCasablanca,portLiverpool,3),
                new Route(portCasablanca,portCapetown,6)};

            var expectedLevel2 = new List<IRoute>() {
                new Route(portLiverpool,portCasablanca,3),
                new Route(portLiverpool,portCapetown,6)};

            Dictionary<int, List<IRoute>> results = Dijkstra.BreadthFirstSearchRoutes(portBuenosAires, _routeRepository.GetAllRoutes());            
            
            Assert.IsTrue(results[0].Intersect(expectedLevel0).Count() == expectedLevel0.Count());
            Assert.IsTrue(results[1].Intersect(expectedLevel1).Count() == expectedLevel1.Count());
            Assert.IsTrue(results[2].Intersect(expectedLevel2).Count() == expectedLevel2.Count());
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithMaxNumberOfThreeStops()
        {
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var numberOfRoutes = Dijkstra.GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStops(portLiverpool, portLiverpool, _routeRepository.GetAllRoutes(), 3);
            Assert.That(numberOfRoutes, Is.EqualTo(2));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromBuenosAiresToLiverpoolWithFourStops()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var results = Dijkstra.GetNumberOfRoutesBetweenPortsWithNumberOfStops(portBuenosAires, portLiverpool, _routeRepository.GetAllRoutes(), 4);

            Assert.That(results, Is.EqualTo(1));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithJourneyTimeEqualOrLessThanTwentyFive()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var routes = _routeRepository.GetAllRoutes();
            var results = Dijkstra.GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(portLiverpool, portLiverpool, _routeRepository.GetAllRoutes(), 25);

            Assert.That(results, Is.EqualTo(3));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromBuenosAiresToLiverpoolWithFourStopsv2()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var results = Dijkstra.GetNumberOfRoutesBetweenPortsWithNumberOfStopsv2(portBuenosAires, portLiverpool, _routeRepository.GetAllRoutes(), 4);

            Assert.That(results, Is.EqualTo(1));
        }

        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithMaxNumberOfThreeStopsv2()
        {
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            var numberOfRoutes = Dijkstra.GetNumberOfRoutesBetweenPortsWithMaximumNumberOfStopsv2(portLiverpool, portLiverpool, _routeRepository.GetAllRoutes(), 3);
            Assert.That(numberOfRoutes, Is.EqualTo(2));
        }



        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithJourneyTimeEqualOrLessThanTwentyFive2()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");            
            IPort portLiverpool = _portRepository.GetPort("Liverpool");

            var routes = _routeRepository.GetAllRoutes();
            var results = Dijkstra.GetNumberOfRoutesBetweenPortsWithMaxJourneyTime(portBuenosAires, portLiverpool, _routeRepository.GetAllRoutes(), 25);

            Assert.That(results, Is.EqualTo(1));
        }


        [Test]
        public void ShouldGetNumberOfJourneysFromLiverpoolToLiverpoolWithJourneyTimeEqualOrLessThanTwentyFivemin()
        {
            IPort portBuenosAires = _portRepository.GetPort("Buenos Aires");
            IPort portNy = _portRepository.GetPort("New York");
            IPort portLiverpool = _portRepository.GetPort("Liverpool");
            IPort portCasablanca = _portRepository.GetPort("Casablanca");
            IPort portCapetown = _portRepository.GetPort("Cape Town");

            var routes = _routeRepository.GetAllRoutes();

            var results = Dijkstra.BreadthFirstSearchRoutesWithPortRepetition(portNy, portNy, routes, 50);
            int totaltime = int.MaxValue;
            foreach (var j in results)
            {
                var currentTime = j.GetTime(_routeRepository);
                if (currentTime < totaltime)
                    totaltime = currentTime;
            }

            Assert.That(results, Is.EqualTo(3));
        }
    }
}
