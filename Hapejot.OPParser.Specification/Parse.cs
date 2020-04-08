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
    public class Parse
    {
        [Fact]
        public void Test1()
        {
            // arrange
            var s = new TokenStream(new StringReader("a=\"xx\""));
            var p = new ExpressionParser(s);

            // act
            var x = p.Parse();

            // assert
            x.Arity.Should().Be(2);
            x.Name.Should().Be("EQ");
            x[0].Name.Should().Be("a");
            x[1].Name.Should().Be("STRING");
            x[1][0].Name.Should().Be("xx");
        }
    }
}
