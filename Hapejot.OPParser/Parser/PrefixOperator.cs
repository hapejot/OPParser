using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{

    public class PrefixOperator:Operator
    {
        public override Operator nud()
        {
            this[0] = parser.Expression(70);
            return this;
        }
    }
}
