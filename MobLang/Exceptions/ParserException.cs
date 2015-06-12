using System;
using MobLang.Parser;

namespace MobLang.Exceptions
{
    public class ParserException : Exception
    {
        internal ParserException(Tokenizer tokenizer, string error)
            : base("At position " + tokenizer.Position + ": " + error)
        {
        }
    }
}