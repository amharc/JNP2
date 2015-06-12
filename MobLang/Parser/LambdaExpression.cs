using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class LambdaExpression : Expression
    {
        public readonly List<Pattern> Arguments;
        public readonly Expression Body;

        public LambdaExpression(List<Pattern> arguments, Expression body)
        {
            Arguments = arguments;
            Body = body;
        }

        public override LazyValue Evaluate(Environment env)
        {
            return Arguments.Reverse<Pattern>().Aggregate<Pattern, System.Func<Environment, LazyValue>>(
                local => Body.Evaluate(local),
                (body, argumentPattern) => local => new FunctionValue(argument =>
                {
                    if (!argumentPattern.Match(argument, ref local))
                        throw new UnsuccessfulMatchException(argument);
                    return body(local);
                }
            ))(env.Local());
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            var real = Arguments.Reverse<Pattern>().Aggregate<Pattern, System.Func<Environment, Type>>(
                local => Body.Infer(local, subst),
                (body, argumentPattern) => local =>
                {
                    var pattern = argumentPattern.Infer(local, subst);
                    return new FunctionType(pattern, body(local));
                }
                )(env.Local());

            return real.Perform(subst);
        }

        public override string ToString()
        {
            var hdr = "fun";
            if (Arguments.Count > 0)
                hdr = hdr + " " + string.Join(" ", Arguments.Select(x => string.Format("({0})", x)));
            return string.Format("{0} as ({1})", hdr, Body);
        }
    }
}