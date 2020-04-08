using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{
	public class ListOperator:ParenOperator
	{
	    public override Operator nud()
	    {
	        MethodTracer.Enter();
	        ParseTreeNode n = null;
	        int i = 0;
	        while(parser.Token != null && parser.Token.value != Closing)
	        {
	            n = parser.Expression(0);
	            this[i++] = n;
	            if( parser.Token.type == TokenType.Symbol && parser.Token.value == "|" )
	            {
	                parser.Advance();
	                BuildComprehension();
	                break;
	            }
	            if( parser.Token != null && parser.Token.value == Separator)
	                parser.Advance(TokenType.Symbol, Separator);
	        }
	        if( parser.Token != null )
	            parser.Advance(TokenType.Symbol, Closing);
	        MethodTracer.Exit();
	        return this;
	    }
	    void BuildComprehension()
	    {
	        MethodTracer.Enter();
	        while(true)
	        {
	            MethodTracer.LogMessage("{0}", parser.Token);
	            if(parser.Token.type != TokenType.Identifier){
	                MethodTracer.LogWarning("First part in comprehension must be an identifier.");
	                break;
	            }
	            Name = "LISTCOMPREHENSION";
	            this[1] = new Operator() { Name = parser.Token.value };
	            parser.Advance();
	            parser.Advance(TokenType.Symbol, ":");
	            this[2] = parser.Expression(0);
	        }
	        MethodTracer.Exit();
	    }
	}
}
