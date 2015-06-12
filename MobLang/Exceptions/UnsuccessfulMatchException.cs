using System;
using MobLang.Interpreter;

namespace MobLang.Exceptions
{
    public class UnsuccessfulMatchException : Exception
    {
        internal UnsuccessfulMatchException(LazyValue value)
            : base(string.Format("Could not match {0}", value))
        {
        }
    }
}