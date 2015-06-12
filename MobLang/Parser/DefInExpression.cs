using System.Collections.Generic;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class DefInExpression : DefExpression
    {
        public DefInExpression(List<DefClause> clauses, Expression inner)
            : base(clauses)
        {
            Inner = inner;
        }

        public Expression Inner { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            var local = env.Local();
            AddToEnvironment(local);
            return Inner.Evaluate(local);
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            var local = env.Local();
            try
            {
                base.Infer(local, subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In the def-clause of a def-in-expression: " + this);
                throw;
            }

            try
            {
                return Inner.Infer(local, subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In the in-clause of a def-in-expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " in " + Inner;
        }
    }
}