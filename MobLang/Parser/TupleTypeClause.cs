using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class TupleTypeClause : TypeClause
    {
        public readonly List<TypeClause> Arguments;

        public TupleTypeClause(List<TypeClause> arguments)
        {
            Arguments = arguments;
        }

        public override Type Parse(Environment env, Substitution subst, bool allowUnbound = false)
        {
            try
            {
                return new TupleType(Arguments.Select(x => x.Parse(env, subst, allowUnbound)).ToList());
            }
            catch (StackedException ex)
            {
                ex.Push("In a tuple type clause: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join(", ", Arguments));
        }
    }
}