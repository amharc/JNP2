using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class TupleExpression : Expression
    {
        public TupleExpression(List<Expression> arguments)
        {
            Arguments = arguments;
        }

        public List<Expression> Arguments { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            return new TupleValue(Arguments.Select(arg => arg.Evaluate(env.Local())).ToList());
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                return new TupleType(Arguments.Select(x => x.Infer(env, subst)).ToList()).Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In a tuple expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join(", ", Arguments));
        }
    }
}