using System;

namespace MobLang.Exceptions
{
    public class PatternArgumentCountException : Exception
    {
        public PatternArgumentCountException(int expected, int got)
            : base(string.Format("Invalid number of arguments in pattern, expected {0}, but got {1}", expected, got))
        {
        }
    }
}