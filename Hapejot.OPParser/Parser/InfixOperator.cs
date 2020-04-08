using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{
    public class InfixOperator : Operator
    {
        public int bp;
        public override Operator Copy()
        {
            InfixOperator result = (InfixOperator)base.Copy();
            result.bp = this.bp;
            return result;
        }
        public override Operator led(Operator left)
        {
            if (ledd == null)
            {
                this[0] = left;
                this[1] = parser.Expression(bp);
                return this;
            }
            else
            {
                return ledd(this, left);
            }
        }

    }
}
