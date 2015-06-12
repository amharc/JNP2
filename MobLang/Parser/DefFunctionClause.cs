using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class DefFunctionClause : DefClause
    {
        public DefFunctionClause(string name, List<Pattern> arguments, Expression body)
        {
            Name = name;
            Arguments = arguments;
            Body = body;
        }

        public string Name { get; private set; }
        public List<Pattern> Arguments { get; private set; }
        public Expression Body { get; private set; }
        protected Type Type { get; set; }
        private LazyValue LazyValue { get; set; }

        public override void PrepareValueEnvironment(Environment env)
        {
            LazyValue = new LazyValue(Defaults.Bottom);
            env.Bind(Name, LazyValue);
        }

        public override void AddValueToEnvironment(Environment env)
        {
            LazyValue.Value = Arguments.Reverse<Pattern>().Aggregate<Pattern, System.Func<Environment, LazyValue>>(
                outer => Body.Evaluate(outer.Local()),
                (body, argumentPattern) => outer => new FunctionValue(argumentValue =>
                {
                    var local = outer.Local();
                    if (!argumentPattern.Match(argumentValue, ref local))
                        throw new UnsuccessfulMatchException(argumentValue);
                    return body(local);
                })
                )(env.Local()).Value;
        }

        public override void PrepareTypeEnvironment(Environment env, Substitution subst)
        {
            Type = new TypeVariable();
            env.Bind(Name, Type);
        }

        protected void UpdateType(Environment env, Substitution subst, bool generalize)
        {
            var real = Arguments.Reverse<Pattern>().Aggregate<Pattern, System.Func<Environment, Type>>(
                outer => Body.Infer(outer.Local(), subst),
                (body, argumentPattern) => outer =>
                {
                    var local = outer.Local();
                    var pattern = argumentPattern.Infer(local, subst);
                    return new FunctionType(pattern, body(local));
                }
                )(env.Local());

            Type = Type.Instantiate();
            Type.Unify(real, subst);
            Type = Type.Perform(subst);

            env.ValueTypes = env.ValueTypes.Remove(Name);

            if (generalize)
                Type = Type.RemoveRigids().Generalize(env);

            env.Bind(Name, Type);
        }

        public override void AddTypeToEnvironment(Environment env, Substitution subst)
        {
            UpdateType(env, subst, Arguments.Count > 0);
            env.Bind(Name, Type);
        }

        public override string ToString()
        {
            return Arguments.Count == 0 ? 
                string.Format("{0} as {1}", Name, Body) :
                string.Format("{0} {1} as {2}", Name, string.Join(" ", Arguments.Select(x => string.Format("({0})", x))), Body);
        }
    }
}