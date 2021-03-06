﻿using NUnit.Framework;
using TransportOperatorBusiness;

namespace UnitTests
{
    public class RouteTests
    {
        [Test]
        public void ShouldHaveAPort()
        {
            const string expectedName = "new york";
            var port = new Port(expectedName);

            Assert.That(port.Name, Is.EqualTo(expectedName));
        }


        [Test]
        public void ShouldHaveARouteWithOriginAndDestination()
        {
            var portOrigin = new Port("Cape Town");
            var portDestination = new Port("New York");

            IRoute<IPort> route = new Route<IPort>(portOrigin, portDestination,  1);

            Assert.That(route.Origin.Name, Is.EqualTo("Cape Town"));
            Assert.That(route.Destination.Name, Is.EqualTo("New York"));
        }

        [Test]
        public void ShouldHaveARouteTimeInDays()
        {
            var portOrigin = new Port("Cape Town");
            var portDestination = new Port("New York");
            var routeTimeInDays = 5;

            IRoute<IPort> route = new Route<IPort>(portOrigin, portDestination, routeTimeInDays);
            IRoute<IPort> route2 = new Route<IPort>(portOrigin, portDestination, 7);

            Assert.That(route.RouteTimeInDays, Is.EqualTo(5));
            Assert.That(route2.RouteTimeInDays, Is.EqualTo(7));
        }
    }
}