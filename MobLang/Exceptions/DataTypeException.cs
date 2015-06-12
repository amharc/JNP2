using System;

namespace MobLang.Exceptions
{
    public class DataTypeException : Exception
    {
        public DataTypeException(string msg)
            : base(msg)
        {
        }
    }
}