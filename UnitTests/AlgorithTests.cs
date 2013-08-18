using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TransportOperatorBusiness;

namespace UnitTests
{
    [TestFixture]
    class AlgorithTests
    {
        
        //public class DijkstraRouteEngineTests
        

            [Test]
            public void Calculate_A_to_D_given_AB_BC_CD__should_be__ABCD()
            {
                var Results = Engine.CalculateShortestPathBetween(
                    "A",
                    "D",
                    new Path<string>[] {
                new Path<string>() { Source = "A", Destination = "B", Cost = 3 },
                new Path<string>() { Source = "B", Destination = "C", Cost = 3 },
                new Path<string>() { Source = "C", Destination = "D", Cost = 3 }
            });

                Results.Sum(r => r.Cost).Should().Be(9);
                Results.Count.Should().Be(3);

                Results.First().Cost.Should().Be(3);
                Results.First().Source.Should().Be("A");
                Results.First().Destination.Should().Be("B");

                Results.Skip(1).First().Cost.Should().Be(3);
                Results.Skip(1).First().Source.Should().Be("B");
                Results.Skip(1).First().Destination.Should().Be("C");

                Results.Skip(2).First().Cost.Should().Be(3);
                Results.Skip(2).First().Source.Should().Be("C");
                Results.Skip(2).First().Destination.Should().Be("D");
            }

            [Test]
            public void Calculate_A_to_D_given_AB_BC_CD_DE__should_be__ABCD()
            {
                var Results = Engine.CalculateShortestPathBetween(
                    "A",
                    "D",
                    new Path<string>[] {
                new Path<string>() { Source = "A", Destination = "B", Cost = 3 },
                new Path<string>() { Source = "B", Destination = "C", Cost = 3 },
                new Path<string>() { Source = "C", Destination = "D", Cost = 3 },
                new Path<string>() { Source = "D", Destination = "E", Cost = 3 }
            });

                Results.Sum(r => r.Cost).Should().Be(9);
                Results.Count.Should().Be(3);

                Results.First().Cost.Should().Be(3);
                Results.First().Source.Should().Be("A");
                Results.First().Destination.Should().Be("B");

                Results.Skip(1).First().Cost.Should().Be(3);
                Results.Skip(1).First().Source.Should().Be("B");
                Results.Skip(1).First().Destination.Should().Be("C");

                Results.Skip(2).First().Cost.Should().Be(3);
                Results.Skip(2).First().Source.Should().Be("C");
                Results.Skip(2).First().Destination.Should().Be("D");
            }

            [Test]
            public void Calculate_A_to_D_given_AB_AC_AD_AE_BC_CD_DE__should_be__ACD()
            {
                var Results = Engine.CalculateShortestPathBetween(
                    "A",
                    "D",
                    new Path<string>[] {
                new Path<string>() { Source = "A", Destination = "B", Cost = 3 },
                new Path<string>() { Source = "A", Destination = "C", Cost = 3 },
                new Path<string>() { Source = "A", Destination = "D", Cost = 7 }, // set this just above ABC (3+3=6)
                new Path<string>() { Source = "A", Destination = "E", Cost = 3 },
 
                new Path<string>() { Source = "B", Destination = "C", Cost = 3 },
 
                new Path<string>() { Source = "C", Destination = "D", Cost = 3 },
 
                new Path<string>() { Source = "D", Destination = "E", Cost = 3 }
            });

                Results.Sum(r => r.Cost).Should().Be(6);
                Results.Count.Should().Be(2);

                Results.First().Cost.Should().Be(3);
                Results.First().Source.Should().Be("A");
                Results.First().Destination.Should().Be("C");

                Results.Skip(1).First().Cost.Should().Be(3);
                Results.Skip(1).First().Source.Should().Be("C");
                Results.Skip(1).First().Destination.Should().Be("D");
            }

            [Test]
            public void Calculate_A_to_D_given_AB_AC_AD_AE_BC_CD_DE__should_be__AD()
            {
                var Results = Engine.CalculateShortestPathBetween(
                    "A",
                    "D",
                    new Path<string>[] {
                new Path<string>() { Source = "A", Destination = "B", Cost = 3 },
                new Path<string>() { Source = "A", Destination = "C", Cost = 3 },
                new Path<string>() { Source = "A", Destination = "D", Cost = 5 }, // set this just below ABC (3+3=6)
                new Path<string>() { Source = "A", Destination = "E", Cost = 3 },
 
                new Path<string>() { Source = "B", Destination = "C", Cost = 3 },
 
                new Path<string>() { Source = "C", Destination = "D", Cost = 3 },
 
                new Path<string>() { Source = "D", Destination = "E", Cost = 3 }
            });

                Results.Sum(r => r.Cost).Should().Be(5);
                Results.Count.Should().Be(1);

                Results.Single().Cost.Should().Be(5);
                Results.Single().Source.Should().Be("A");
                Results.Single().Destination.Should().Be("D");
            }        
    }


}
