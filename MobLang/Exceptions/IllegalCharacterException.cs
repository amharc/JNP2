using System;

namespace MobLang.Exceptions
{
    public class IllegalCharacterException : Exception
    {
        public IllegalCharacterException(char illegal) : base("Illegal character: " + illegal)
        {
        }
    }
}