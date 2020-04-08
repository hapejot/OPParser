using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hapejot.OPParser
{
    public class ExpressionParser
    {
        private TokenStream _tokens;
        private Token _token;
        private Operator _op;
        private Dictionary<Token, Operator> _top = new Dictionary<Token, Operator>();
        public Token Token
        {
            get { return _token; }
            set { _token = value; }
        }
        public ExpressionParser(TokenStream pStream)
        {
            _tokens = pStream;
            _top[new Token(TokenType.Symbol, "(")] = new ParenOperator() { Name = "PAREN", lbp = 100, parser = this, bp = 1, Closing = ")", Separator=","};
            _top[new Token(TokenType.Symbol, "[")] = new ListOperator() { Name = "LIST", lbp = 100, parser = this, bp = 1, Closing = "]", Separator=","};
            _top[new Token(TokenType.Identifier, "not")] = new PrefixOperator() { Name = "NOT", lbp = 0, parser = this };
            _top[new Token(TokenType.Symbol, "*")] = new InfixOperator() { Name = "MULT", lbp = 60, parser = this, bp = 60 };
            _top[new Token(TokenType.Symbol, "/")] = new InfixOperator() { Name = "DIV", lbp = 60, parser = this, bp = 60 };
            _top[new Token(TokenType.Symbol, "+")] = new InfixOperator() { Name = "PLUS", lbp = 50, parser = this, bp = 50 };
            _top[new Token(TokenType.Symbol, "++")] = new InfixOperator() { Name = "APPEND", lbp = 50, parser = this, bp = 50 };
            _top[new Token(TokenType.Symbol, "-")] = new InfixOperator() { Name = "MINUS", lbp = 50, parser = this, bp = 50 };
            _top[new Token(TokenType.Symbol, ">")] = new InfixOperator() { Name = "GT", lbp = 40, parser = this, bp = 40 };
            _top[new Token(TokenType.Symbol, "=")] = new InfixOperator() { Name = "EQ", lbp = 40, parser = this, bp = 40 };
            _top[new Token(TokenType.Symbol, "<-")] = new InfixOperator() { Name = "ASSIGN", lbp = 40, parser = this, bp = 40 };
            _top[new Token(TokenType.Symbol, "<")] = new InfixOperator() { Name = "LT", lbp = 40, parser = this, bp = 40 };
            _top[new Token(TokenType.Symbol, "<>")] = new InfixOperator() { Name = "NEQ", lbp = 40, parser = this, bp = 40 };
            _top[new Token(TokenType.Identifier, "or")] = new InfixOperator() { Name = "OR", lbp = 20, parser = this, bp = 20 };
            _top[new Token(TokenType.Identifier, "and")] = new InfixOperator() { Name = "AND", lbp = 30, parser = this, bp = 30 };
            _top[new Token(TokenType.Identifier, "in")] = new InfixOperator() { Name = "IN", lbp = 61, parser = this, bp = 61 };
            _top[new Token(TokenType.Identifier, "days")] = new PostfixOperator() { Name = "DAYS", lbp = 70, parser = this };
            _top[new Token(TokenType.Identifier, "months")] = new PostfixOperator() { Name = "MONTHS", lbp = 70, parser = this };
            _top[new Token(TokenType.Symbol, ";")] = new InfixOperator() { Name = "RULEPAIR", lbp = 1, parser = this, bp = 1 };
            _top[new Token(TokenType.Symbol, ".")] = new InfixOperator() { Name = "SELECT", lbp = 110, parser = this, bp = 110 };
            
            _top[new Token(TokenType.Identifier, "function")] = new Operator() {Name="FUNCTION", lbp = 1, parser = this,
                nudd = (Operator o) => {
                    Advance(TokenType.Symbol, "(");
                    int i = 1;
                    while(true)
                    {
                        o[i++] = Expression(0);
                        if(Token.type != TokenType.Symbol || Token.value != ",")
                        {
                            break;
                        }
                        Advance(TokenType.Symbol, ",");
                    }
                    Advance(TokenType.Symbol, ")");
                    o[0] = Expression(1);
                    return o;
                }
            };
        }
        
        public bool EndOfStreamReached
        {
            get { return _tokens.IsEmpty(); }
        }
        public void Advance(TokenType pType, string pValue)
        {
            if (_token == null
                || _token.type != pType
                || _token.value != pValue)
            {
                throw new ParseException(String.Format("expected {0} {1}", pType, pValue));
            }
            Advance();
        }
        public void Advance()
        {
            if (EndOfStreamReached)
            {
                _token = null;
                _op = new Operator() { Name = "", lbp = 0, parser = this };
            }
            else
            {
                _token = _tokens.ReadToken();
                if( _token.type == TokenType.DoubleQuotedString )
                {
                    _op = new Operator() { Name = "STRING" };
                    _op[0] = new Operator() { Name = _token.value };
                }
                else if(_token.type == TokenType.Number )
                {
                    _op = new Operator() { Name = "NUMBER" };
                    _op[0] = new Operator() { Name = _token.value };
                }
                else if(_token.type == TokenType.Identifier || _token.type == TokenType.Symbol)
                {
                    if (_top.ContainsKey(_token))
                    {
                        _op = _top[_token].Copy();
                        _op.parser = this;
                    }
                    else
                    {
                        int lbp = 1;
                        if (_token.type == TokenType.Symbol)
                        {
                            lbp = 0;
                        }
                        _op = new Operator() { Name = _token.value, lbp = lbp, parser = this };
                    }
                }
                else
                {
                    MethodTracer.LogWarning("Unhandled Token:{0}", _token);
                }
            }
        }
        public ParseTreeNode Expression(int rbp)
        {
            Operator t = _op;
            Advance();
            Operator left = t.nud();
            while (rbp < _op.lbp)
            {
                t = _op;
                Advance();
                left = t.led(left);
            }
            return left;
        }
        public ParseTreeNode Parse()
        {
            ParseTreeNode result = null;
            Advance();
            result = Expression(0);
            return result;
        }
    }
}
