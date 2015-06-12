using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class MatchCase
    {
        public readonly Expression Body;
        public readonly Pattern Pattern;

        public MatchCase(Pattern pattern, Expression body)
        {
            Pattern = pattern;
            Body = body;
        }

        public virtual LazyValue Match(LazyValue value, Environment env)
        {
            var local = env.Local();
            if (Pattern.Match(value, ref local))
                return Body.Evaluate(local);
            return null;
        }

        public virtual void Unify(Type argument, Type result, Environment env, Substitution subst)
        {
            try
            {
                var local = env.Local();
                argument.Unify(Pattern.Infer(local, subst), subst);
                result.Unify(Body.Infer(local, subst), subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In a match-with-clause: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return "with " + Pattern + " then " + Body;
        }
    }
}