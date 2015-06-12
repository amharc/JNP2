using System;

namespace MobLang.Exceptions
{
    public class InvalidCharStringLiteralException : Exception
    {
        public InvalidCharStringLiteralException() : base("Invalid character/string literal")
        {
        }
    }
}