using System;

namespace MobLang.Exceptions
{
    public class UnboundNameException : StackedException
    {
        internal UnboundNameException(string name) : base(string.Format("{0} is not bound", name))
        {
        }
    }
}