using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobLang.Exceptions
{
    class SafeStackOverflowException : Exception
    {
        public SafeStackOverflowException() : base("Recursion depth limit exceeded") { }
    }
}
