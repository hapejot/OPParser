using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{
    [Serializable]
    public class Operator : ParseTreeNode
    {
        public ExpressionParser parser;
        public int lbp;

        public virtual Operator nud()
        {
            if (nudd == null)
            {
                return this;
            }
            else
            {
                return nudd(this);
            }
        }

        public LeftDenotationDelegate ledd = null;
        public NullLeftDenotationDelegate nudd = null;

        public virtual Operator led(Operator left)
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
        public virtual Operator Copy()
        {
            Operator result = null;
            result = (Operator)Activator.CreateInstance(this.GetType());
            result.Name = this.Name;
            result.lbp = this.lbp;
            result.ledd = this.ledd;
            result.nudd = this.nudd;
            return result;
        }
    }
}
