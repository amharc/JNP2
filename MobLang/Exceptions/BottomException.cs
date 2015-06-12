using System;

namespace MobLang.Exceptions
{
    public class BottomException : Exception
    {
        public BottomException() : base("Bottom was evaluated")
        {
        }
    }
}