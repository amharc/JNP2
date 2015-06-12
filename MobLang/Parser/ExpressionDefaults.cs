using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobLang.Parser
{
    static class ExpressionDefaults
    {
        public static readonly VariableExpression Nil = new VariableExpression("Nil");
        public static readonly VariableExpression Cons = new VariableExpression("Cons");
    }
}
