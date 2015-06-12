using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class MatchExpression : Expression
    {
        public readonly List<MatchCase> Patterns;
        public readonly Expression Value;

        public MatchExpression(Expression value, List<MatchCase> patterns)
        {
            Value = value;
            Patterns = patterns;
        }

        public override LazyValue Evaluate(Environment env)
        {
            return new ApplicationValue(
                new FunctionValue(value =>
                {
                    var result = Patterns.Select(pattern => pattern.Match(value, env))
                        .FirstOrDefault(res => res != null);
                    if (result == null)
                        throw new UnsuccessfulMatchException(value);
                    return result;
                }),
                Value.Evaluate(env.Local())
                );
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                var value = Value.Infer(env.Local(), subst);
                Type result = new TypeVariable();
                foreach (var pattern in Patterns)
                {
                    pattern.Unify(value, result, env, subst);
                }

                return result.Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In a match expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            var hdr = "match " + Value;
            if (Patterns.Count == 0)
                return hdr;
            else
                return hdr + " " + string.Join(" ", Patterns);
        }
    }
}