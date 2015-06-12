using System;
using System.Collections.Generic;
using MobLang.TypeSystem;

namespace MobLang.Exceptions
{
    public abstract class StackedException : Exception
    {
        public readonly List<string> Trace;
        internal readonly TypePrettyPrinter PrettyPrinter;

        internal StackedException(string message) : this(message, new TypePrettyPrinter()) { }

        internal StackedException(string message, TypePrettyPrinter printer) : base(message)
        {
            Trace = new List<string>();
            PrettyPrinter = printer;
        }

        public void Push(string message)
        {
            Trace.Add(message);
        }
    }
}