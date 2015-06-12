using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class DataPattern : Pattern
    {
        public readonly List<Pattern> Arguments;
        public readonly string Constructor;

        public DataPattern(string constructor, List<Pattern> arguments)
        {
            Constructor = constructor;
            Arguments = arguments;
        }

        public override bool Match(LazyValue value, ref Environment env)
        {
            var local = env.Local();
            var data = (DataValue) value.WHNFValue;

            var ok = data.Constructor.Equals(env.Constructors[Constructor]) &&
                     !Arguments.Zip(data.Arguments, (pat, val) => pat.Match(val, ref local)).Contains(false);

            if (ok)
                env = local;
            return ok;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            var constructor = env.Constructors[Constructor];
            if (constructor.Arity != Arguments.Count)
                throw new PatternArgumentCountException(constructor.Arity, Arguments.Count);

            Type self = new TypeVariable();

            var list = Arguments.Select(arg => arg.Infer(env, subst)).ToList();

            var whole = list.Reverse<Type>().Aggregate(self, (res, arg) => new FunctionType(arg, res));
            whole.Unify(constructor.Type.Instantiate(), subst);
            return self.Perform(subst);
        }

        public override string ToString()
        {
            return Arguments.Aggregate(Constructor, (x, y) => x + " (" + y + ")");
        }
    }
}