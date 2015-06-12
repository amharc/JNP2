using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class ConditionalMatchCase : MatchCase
    {
        public readonly Expression Condition;

        public ConditionalMatchCase(Pattern pattern, Expression condition, Expression body)
            : base(pattern, body)
        {
            Condition = condition;
        }

        public override LazyValue Match(LazyValue value, Environment env)
        {
            var local = env.Local();

            if (!Pattern.Match(value, ref local))
                return null;

            var condition = (DataValue) Condition.Evaluate(local.Local()).WHNFValue;
            return condition.CompareTo(Defaults.True) == 0 ? Body.Evaluate(local) : null;
        }

        public override void Unify(Type argument, Type result, Environment env, Substitution subst)
        {
            try
            {
                var local = env.Local();
                argument.Unify(Pattern.Infer(local, subst), subst);
                Condition.Infer(local, subst).Unify(TypeDefaults.Bool, subst);
                result.Unify(Body.Infer(local, subst), subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In a conditional match case: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return string.Format("with {0} if {1} then {2}", Pattern, Condition, Body);
        }
    }
}