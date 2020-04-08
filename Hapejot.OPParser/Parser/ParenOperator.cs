using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{

    public class ParenOperator:PrefixOperator
    {
        public string Closing;
        public string Separator;
        public int bp ;
        public override Operator Copy()
        {
            ParenOperator result = (ParenOperator)base.Copy();
            result.Closing = Closing;
            result.Separator = Separator;
            result.bp = bp;
            return result;
        }
        public override Operator led(Operator left)
        {
            MethodTracer.LogMessage("Closing:{0},Separator:{1}", Closing, Separator);
            if (ledd == null)
            {
                Name="APPLY";
                this[0] = left;
                if (parser.Token != null && parser.Token.value != Closing)
                {
                    int i = 1;
                    while (true)
                    {
                        this[i] = parser.Expression(0);
                        i++;
                        if( parser.Token != null )
                            MethodTracer.LogMessage("Expression Sep: {0}", parser.Token.value == Separator);
                        if (parser.Token == null || parser.Token.value != Separator)
                            break;
                        parser.Advance(TokenType.Symbol, Separator);
                    }
                    parser.Advance(TokenType.Symbol, Closing);
                }
                return this;
            }
            else
            {
                return ledd(this, left);
            }
        }
        public override Operator nud()
        {
            MethodTracer.Enter();
            ParseTreeNode n = null;
            int i = 0;
            while(parser.Token != null && parser.Token.value != Closing)
            {
                n = parser.Expression(0);
                MethodTracer.LogMessage("Expression in List:{0}", n);
                this[i++] = n;
                if( parser.Token != null && parser.Token.value == Separator)
                    parser.Advance(TokenType.Symbol, Separator);
            }
            if( parser.Token != null )
                parser.Advance(TokenType.Symbol, Closing);
            MethodTracer.Exit();
            return this;
        }
    }
}
