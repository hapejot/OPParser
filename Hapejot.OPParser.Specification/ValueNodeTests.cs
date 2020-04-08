using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hapejot.OPParser.Specification
{
    public class ValueNodeTests
    {
        [Fact]
        public void Test1()
        {
            // arrange
            var n = new ValueNode("test");

            // act

            // assert
            n.Name.Should().Be("test");
        }

        [Fact]
        public void Test2()
        {
            // arrange
            var n = new ValueNode("test", "a", 1);

            // act

            // assert
            n.Name.Should().Be("test");
            n.Arg[0].Should().Be("a");
            n.Arg[1].Should().Be(1);
        }
    }
}
