using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class TuplePattern : Pattern
    {
        public readonly List<Pattern> List;

        public TuplePattern(List<Pattern> list)
        {
            List = list;
        }

        public override bool Match(LazyValue value, ref Environment env)
        {
            var local = env.Local();
            var tuple = (TupleValue) value.WHNFValue;

            var ok = !List.Zip(tuple.List, (pat, val) => pat.Match(val, ref local)).Contains(false);

            if (ok)
                env = local;
            return ok;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                var list = List.Select(arg => arg.Infer(env, subst)).ToList();

                return new TupleType(list);
            }
            catch (StackedException ex)
            {
                ex.Push("In a tuple pattern: " + this);
                throw;
            }
        }
        public override string ToString()
        {
            return string.Format("({0})", string.Join(", ", List));
        }
    }
}