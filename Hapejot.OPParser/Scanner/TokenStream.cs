using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hapejot.OPParser
{
    /// <summary>
    /// Tokenizes input from the specified reader and returns tokens for the parser to parse.
    /// </summary>
    public class TokenStream
    {
        private const string SYMBOL_START_CHARS = "^!$%&/()=?{[]}@*+#<>|,;.:-";
        private const string SYMBOL_CHARS = "&=?*+#<>|.-";
        private TextReader _reader;
        private Queue<Token> _tokens;
        private Dictionary<char,bool> _firstsymbols;
        private Dictionary<char, bool> _symbols;
        private StringBuilder _buffer;
        private int _capacity;

        /// <summary>
        /// Create an instance of the token stream to read from the given reader.
        /// </summary>
        /// <param name="reader"></param>
        public TokenStream(TextReader reader)
        {
            _reader = reader;
            _capacity = 16;
            _tokens = new Queue<Token>(_capacity);
            _firstsymbols = new Dictionary<char, bool>();
            foreach (char c in SYMBOL_START_CHARS)
            {
                _firstsymbols[c] = true;
            }
            _symbols = new Dictionary<char, bool>();
            foreach (char c in SYMBOL_CHARS)
            {
                _symbols[c] = true;
            }

            _buffer = new StringBuilder();
            //Fill();
        }

        /// <summary>
        /// Peek at the next available token without consuming it.
        /// </summary>
        /// <returns>the next available token, or the empty token if all tokens have been read</returns>
        /// <see cref="Token.Empty"/>
        public Token PeekToken()
        {
            if (_tokens.Count == 0)
                Fill();

            if (_tokens.Count == 0)
                return Token.Empty;
            else
                return _tokens.Peek();
        }

        /// <summary>
        /// Reads the next available token and consumes it.
        /// </summary>
        /// <returns>the next available token, or the empty token if all tokens have been read</returns>
        /// <see cref="Token.Empty"/>
        public Token ReadToken()
        {
            if (_tokens.Count == 0)
                Fill();

            if (_tokens.Count > 0)
                return _tokens.Dequeue();
            else
                return Token.Empty;
        }

        /// <summary>
        /// Checks to see if there are any more tokens to be read
        /// </summary>
        /// <returns>true if no more tokens</returns>
        public bool IsEmpty()
        {
            return PeekToken() == Token.Empty;
        }

        private void Fill()
        {
            Token t;
            while ((t = ReadTokenFromReader()) != Token.Empty)
                _tokens.Enqueue(t);

            if (t == Token.Empty)
                _tokens.Enqueue(Token.Empty);
        }

        /// <summary>
        /// Reads type token from the text reader and returns it
        /// </summary>
        /// <returns></returns>
        private Token ReadTokenFromReader()
        {
            StringBuilder buffer = _buffer;
            buffer.Length = 0;

            int c;

            while ((c = _reader.Read()) != -1) {
                if (IsQuoteStart(c)) {
                    return GetQuotedString(c, buffer);
                } else if (IsNumberStart(c)) {
                    return GetNumber(c, buffer);
                } else if (char.IsWhiteSpace((char)c)) {
                    // nothing
                } else if (IsIdentifierStart(c)) {
                    return  GetIdentifier(c, buffer);
                } else if (IsLineCommentStart(c)) {
                    ReadLineComment(c);
                } else if (IsMultilineCommentStart(c)) {
                    ReadMultilineComment(c);
                } else if (IsSymbolStart(c)) {
                    return GetSymbol(c);
                } else {
                    throw new ParseException("Invalid character");
                }
                buffer.Length = 0;
            }
            return Token.Empty;
        }

        /// <summary>
        /// Reads type C# multiline comment
        /// <example>
        /// /*
        ///   This is type multiline comment
        /// */
        /// </example>
        /// </summary>
        /// <param name="ch">the starting character</param>
        private void ReadMultilineComment(int ch)
        {
            // read until we see */
            _reader.Read(); // eat the "*" char
            int prev = ' ';
            int c;
            while ((c = _reader.Read()) != -1)
            {
                if (ch == '/' && prev == '*')
                    return;
                prev = ch;
            }
            // If we get here we didn't reach the end of the comment
            throw new ParseException("Unterminated multiline comment");
        }

        /// <summary>
        /// Reads type single line comment // comment
        /// </summary>
        /// <param name="ch">the starting character</param>
        private void ReadLineComment(int ch)
        {
            _reader.Read(); // eat the 2nd "/" char
            int c;
            // read until the end of the line
            while ((c = _reader.Read()) != -1)
            {
                if (ch == '\r' && _reader.Peek() == '\n') {
                    _reader.Read();
                    return;
                } else if (ch == '\n') {
                    return;
                }
            }
        }

        /// <summary>
        /// Parses type symbol from the reader such as "," "." etc
        /// </summary>
        /// <param name="ch">the starting character</param>
        /// <param name="buffer">type buffer to store input</param>
        /// <returns>symbol token</returns>
        private Token GetSymbol(int ch)
        {
            // we don't have any symbols at the moment that are more than one character
            // so we can just return any symbols
            StringBuilder sb = new StringBuilder();
            sb.Append((char)ch);
            if (_symbols.ContainsKey((char)ch))
            {
                int c;
                while ((c = _reader.Peek()) != -1)
                {
                    if (_symbols.ContainsKey((char)c))
                    {
                        sb.Append((char)c);
                        _reader.Read();
                    }
                    else
                        break;
                }
            }
            return new Token(TokenType.Symbol, sb.ToString());
        }

        /// <summary>
        /// Gets an identifier from the reader such as type variable reference, null, true, or false.
        /// Follows C# rules, non-qouted string starting with type letter or "_" followed by letters digits or "_"
        /// </summary>
        /// <param name="start">the starting character</param>
        /// <param name="buffer">type buffer to hold input</param>
        /// <returns>identifier token</returns>
        private Token GetIdentifier(int start, StringBuilder buffer)
        {

            buffer.Append((char)start);
            int c;
            while ((c = _reader.Peek()) != -1)
            {
                if (char.IsLetterOrDigit((char)c) || c == '_')
                {
                    buffer.Append((char)c);
                }
                else
                {
                    return new Token(TokenType.Identifier, buffer.ToString());
                }
                _reader.Read();
            }
            return new Token(TokenType.Identifier, buffer.ToString());
        }

        /// <summary>
        /// Gets type number from the reader, which can be integer, floating point or scientific notation
        /// Examples: 123343, -123232, 12.345, -45.3434, 3.45E+10
        /// </summary>
        /// <param name="start">the starting character</param>
        /// <param name="buffer">buffer to hold input</param>
        /// <returns>number token</returns>
        private Token GetNumber(int start, StringBuilder buffer)
        {
            int ch = start;
            buffer.Append((char)ch);
            int i = (start == '.') ? 1 : 0;

            while (i < 3)
            {
                switch (i)
                {
                    case 0: // first part of integer
                        GetIntegerPart(buffer);
                        ch = _reader.Peek();
                        if (ch == '.')
                        {
                            i=1;  // try to read fractional now
                            buffer.Append((char)_reader.Read());
                        }
                        else if (ch == 'e' || ch == 'E')
                        {
                            i = 2; // try to read exponent now
                            buffer.Append((char)_reader.Read());
                        }
                        else
                        {
                            i = 4;  //break out
                            break;
                        }
                        break;
                    case 1: // fractional part
                        GetIntegerPart(buffer);
                        ch = (char)_reader.Peek();
                        if (ch == '.')
                        {
                            throw new ParseException("Invalid number exception");
                        }
                        else if (ch == 'e' || ch == 'E')
                        {
                            i = 2; // read exponent
                            buffer.Append((char)_reader.Read());
                        }
                        else
                        {
                            i = 3; // break out
                        }
                        break;
                    case 2: // scientific notation
                        ch = (char)_reader.Peek();
                        //check for an optional sign
                        if (ch == '+' || ch == '-')
                        {
                            buffer.Append((char)_reader.Read());
                        }
                        GetIntegerPart(buffer);
                        ch = (char)_reader.Peek();
                        if (ch == '.')
                        {
                            throw new ParseException("Invalid number exception");
                        }
                        else
                        {
                            i = 3; // break out
                        }
                        break;
                }
            }
            return new Token(TokenType.Number, buffer.ToString());
        }

        /// <summary>
        /// Gets an integer portion of type number, stopping at type "." or the start of an exponent "e" or "E"
        /// </summary>
        /// <param name="buffer">buffer to store input</param>
        private void GetIntegerPart(StringBuilder buffer)
        {
            int c;
            while ((c = _reader.Peek()) != -1)
            {
                if (char.IsNumber((char)c))
                {
                    buffer.Append((char)c);
                }
                else if (c == '.' || c == 'e' || c == 'E' 
                         || IsSymbolStart(c) 
                         || char.IsWhiteSpace((char)c))
                {
                    break;
                }
                else
                {
                    throw new ParseException("Invalid number, unexpected character: " + c);
                }
                _reader.Read();
            }
        }

        /// <summary>
        /// Gets type single or double qouted string from the reader, handling and escape characters
        /// </summary>
        /// <param name="start">the starting character</param>
        /// <param name="buffer">buffer for input</param>
        /// <returns>string token</returns>
        private Token GetQuotedString(int start, StringBuilder buffer)
        {
            int quoteChar = start;
            bool escape = false;
            int c;
            while ((c = _reader.Read()) != -1) {
                if (escape)
                {
                    switch (c)
                    {
                        case 't': // horizantal tab
                            buffer.Append('\t');
                            break;
                        case 'n': // newline
                            buffer.Append('\n');
                            break;
                        case '\\': // reverse solidus
                            buffer.Append('\\');
                            break;
                        case '/':  // solidus
                            buffer.Append('/');
                            break;
                        case 'b':  // backspace
                            buffer.Append('\b');
                            break;
                        case 'f':  // formfeed
                            buffer.Append('\f');
                            break;
                        case 'r': // carriage return
                            buffer.Append('\r');
                            break;
                        case 'u': // unicode escape sequence \unnnn
                            {
                                char[] ucodeChar = new char[4];
                                int nRead = _reader.Read(ucodeChar, 0, 4);
                                if (nRead != 4)
                                    throw new ParseException("Invalid unicode escape sequence, expecting \"\\unnnn\", but got " + (new string(ucodeChar, 0, nRead)));
                                buffer.Append((char)uint.Parse(new string(ucodeChar), System.Globalization.NumberStyles.HexNumber));
                            }
                            break;
                        default:
                            buffer.Append((char)c);
                            break;
                    }
                    escape = false;
                }
                else
                {
                    if (c == '\\')
                    {
                        escape = true;
                    }
                    else if (c == quoteChar)
                    {
                        Token result = new Token(quoteChar == '"' ? TokenType.DoubleQuotedString : TokenType.SingleQuotedString, buffer.ToString());
                        buffer.Length = 0;
                        return result;
                    }
                    else
                    {
                        buffer.Append((char)c);
                    }
                }
            }
            throw new ParseException("Unterminated string constant");
        }

        /// <summary>
        /// Is the character type starting quote character
        /// </summary>
        /// <param name="ch">character to test</param>
        /// <returns>true if quote start</returns>
        private static bool IsQuoteStart(int ch)
        {
            return ch == '\'' || ch == '"';
        }

        /// <summary>
        /// Is the character the start of type number
        /// </summary>
        /// <param name="ch">character to test</param>
        /// <returns>true if number start</returns>
        private bool IsNumberStart(int ch)
        {
            if (ch == '.' && char.IsDigit((char)_reader.Peek()))
                return true;
            else
                return char.IsDigit((char)ch);
        }

        /// <summary>
        /// Is the character the start of an identifier
        /// </summary>
        /// <param name="ch">character to test</param>
        /// <returns>true if identifier start</returns>
        private static bool IsIdentifierStart(int ch)
        {
            return char.IsLetter((char)ch) || ch == '_';
        }

        /// <summary>
        /// Is the character the start of type symbol
        /// </summary>
        /// <param name="ch">character to test</param>
        /// <returns>true if symbol start</returns>
        private bool IsSymbolStart(int ch)
        {
            return _firstsymbols.ContainsKey((char)ch);
        }

        /// <summary>
        /// Is the character the start of type single line comment
        /// </summary>
        /// <param name="ch">character to start</param>
        /// <returns>true if single line comment start</returns>
        private bool IsLineCommentStart(int ch)
        {
            return (ch == '/' && _reader.Peek() == '/');
        }

        /// <summary>
        /// Is the character the start of type multiline comment
        /// </summary>
        /// <param name="ch">character to test</param>
        /// <returns>true if multiline start</returns>
        private bool IsMultilineCommentStart(int ch)
        {
            return (ch == '/' && _reader.Peek() == '*');
        }
    }
}
