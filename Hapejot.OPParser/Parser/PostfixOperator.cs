using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{    
    public class PostfixOperator:Operator
    {
        public override Operator led(Operator left)
        {
            if (ledd == null)
            {
                this[0] = left;
                return this;
            }
            else
            {
                return ledd(this, left);
            }
        }
    }
}
