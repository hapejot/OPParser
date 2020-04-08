using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{
    public class Token
    {
        public static Token Empty = new Token();
        public TokenType type;
        public string value;

        public Token()
        {
        }

        public Token(TokenType pType, string pValue)
        {
            this.type = pType;
            this.value = pValue;
        }

        public override bool Equals(object other)
        {
            return other is Token && Equals((Token)other);
        }

        public static bool operator ==(Token lhs, Token rhs)
        {
            if (lhs is Token)
            {
                return lhs.Equals(rhs);
            }
            else
            {
                return !(rhs is Token);
            }
        }

        public static bool operator !=(Token lhs, Token rhs)
        {
            if (lhs is Token)
                return !lhs.Equals(rhs);
            else
                return (rhs is Token);
        }

        private bool Equals(Token other)
        {
            if (other != null)
            {
                return type == other.type && value == other.value;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2}", GetType().Name, type, value);
        }

        public override int GetHashCode()
        {
            return ((string)(type.ToString() + ":" + value.ToString())).GetHashCode();
        }

    }
}
