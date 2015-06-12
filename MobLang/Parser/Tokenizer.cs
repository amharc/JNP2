using System;
using System.Linq;
using System.Text;
using MobLang.Exceptions;

namespace MobLang.Parser
{
    public class Tokenizer
    {
        public static readonly char[] StandaloneChars = {'(', ')', ',', '[', ']'};

        public Tokenizer(string source)
        {
            Source = source;
            Position = 0;
            Advance();
        }

        public string Source { get; private set; }
        public int Position { get; private set; }

        private bool HasNext
        {
            get { return Position < Source.Length; }
        }

        private char NextChar
        {
            get { return Source[Position]; }
        }

        public string Token { get; private set; }

        private void SkipSpaces()
        {
            while (HasNext && char.IsWhiteSpace(NextChar))
                Position++;
        }

        public string NextSpacing()
        {
            return TakeWhile(char.IsWhiteSpace, false);
        }

        private string TakeWhile(Predicate<char> predicate, bool advance = true)
        {
            var newPos = Position;
            while (newPos < Source.Length && predicate(Source[newPos]))
                ++newPos;

            var res = Source.Substring(Position, newPos - Position);
            if(advance)
                Position = newPos;

            return res;
        }

        public bool IsSymbol(char x)
        {
            return (char.IsPunctuation(x) || char.IsSymbol(x)) && !StandaloneChars.Contains(x) && x != '\'' && x != '\"';
        }

        public string Advance()
        {
            var old = Token;

            SkipSpaces();
            if (!HasNext)
                Token = null;
            else
                switch (NextChar)
                {
                    case '"':
                        Position++;
                        var sb = new StringBuilder("\"");
                        bool escaped = false, done = false;
                        while (HasNext)
                        {
                            if (NextChar == '\\')
                            {
                                if (escaped)
                                {
                                    sb.Append('\\');
                                }
                                else
                                {
                                    escaped = true;
                                }
                            }
                            else if (escaped)
                            {
                                escaped = false;
                                switch (NextChar)
                                {
                                    case 'n':
                                        sb.Append("\n");
                                        break;
                                    case 'r':
                                        sb.Append("\r");
                                        break;
                                    case 't':
                                        sb.Append("\t");
                                        break;
                                    case '"':
                                        sb.Append("\"");
                                        break;
                                    default:
                                        throw new InvalidCharStringLiteralException();
                                }
                            }
                            else if (NextChar == '"')
                            {
                                Position++;
                                done = true;
                                break;
                            }
                            else
                            {
                                sb.Append(NextChar);
                            }

                            Position++;
                        }
                        Token = sb.ToString();
                        if (!done)
                            throw new InvalidCharStringLiteralException();
                        break;
                    case '\'':
                        Position++;
                        if (!HasNext)
                            throw new InvalidCharStringLiteralException();

                        Token = "'" + NextChar;

                        Position++;
                        if (!HasNext || NextChar != '\'')
                            throw new InvalidCharStringLiteralException();
                        Position++;
                        break;
                    default:
                        if (StandaloneChars.Contains(NextChar))
                        {
                            Token = NextChar.ToString();
                            Position++;
                        }
                        else if (char.IsLetter(NextChar) || NextChar == '_')
                            Token = TakeWhile(x => char.IsLetterOrDigit(x) || x == '_');
                        else if (char.IsNumber(NextChar))
                            Token = TakeWhile(char.IsNumber);
                        else if (IsSymbol(NextChar))
                            Token = TakeWhile(IsSymbol);
                        else
                            throw new IllegalCharacterException(NextChar);
                        break;
                }

            return old;
        }
    }
}