using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobLang.Exceptions;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    class FunctionTypeClause : TypeClause
    {
        public readonly TypeClause Argument, Result;

        public FunctionTypeClause(TypeClause argument, TypeClause result)
        {
            Argument = argument;
            Result = result;
        }

        public override Type Parse(Environment env, Substitution subst, bool allowUnbound = false)
        {
            try
            {
                var argument = Argument.Parse(env, subst, allowUnbound);
                var result = Result.Parse(env, subst, allowUnbound);
                return new FunctionType(argument, result);
            }
            catch (StackedException ex)
            {
                ex.Push("In a function type clause: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return Argument + " -> " + Result;
        }
    }
}
