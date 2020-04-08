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
    public class Scanner
    {
        [Fact]
        void Test1()
        {
            // arrange
            TokenStream ts = new TokenStream(new StringReader("a=\"xx\""));

            // act
            var tok1 = ts.ReadToken();
            var tok2 = ts.ReadToken();
            var tok3 = ts.ReadToken();

            // assert
            tok1.type.Should().Be(TokenType.Identifier);
            tok2.type.Should().Be(TokenType.Symbol);
            tok3.type.Should().Be(TokenType.DoubleQuotedString);

            tok1.value.Should().Be("a");
            tok2.value.Should().Be("=");
            tok3.value.Should().Be("xx");
        }
    }
}
