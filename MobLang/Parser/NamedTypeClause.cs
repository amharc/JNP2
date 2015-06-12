using System;
using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    internal class NamedTypeClause : TypeClause
    {
        public readonly List<TypeClause> Arguments;
        public readonly string Type;

        public NamedTypeClause(string type, List<TypeClause> arguments)
        {
            Type = type;
            Arguments = arguments;
        }

        public override Type Parse(Environment env, Substitution subst, bool allowUnbound = false)
        {
            try
            {
                if (!env.Types.ContainsKey(Type))
                    throw new DataTypeException(string.Format("Unbound type: {0}", Type));

                var type = env.Types[Type].Instantiate();
                if (!(type is DataType) && Arguments.Count == 0)
                    return type;

                var args = (from arg in Arguments select arg.Parse(env, subst, allowUnbound)).ToList();
                type.Unify(new DataType(Type, args), subst);
                return type;
            }
            catch (StackedException ex)
            {
                ex.Push("In a named type clause: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            if (Arguments.Count == 0)
                return Type;

            return Type + " " + string.Join(" ", Arguments.Select(x => string.Format("({0})", x)));
        }
    }
}