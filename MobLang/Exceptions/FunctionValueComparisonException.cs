using System;

namespace MobLang.Exceptions
{
    public class FunctionValueComparisonException : Exception
    {
        public FunctionValueComparisonException() : base("Attempted to compare functional values")
        {
        }
    }
}