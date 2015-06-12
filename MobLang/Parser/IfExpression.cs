using System.Collections.Generic;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class IfExpression : Expression
    {
        public IfExpression(Expression condition, Expression ifTrue)
            : this(condition, ifTrue, new TupleExpression(new List<Expression>()))
        {
        }

        public IfExpression(Expression condition, Expression ifTrue, Expression ifFalse)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public Expression Condition { get; private set; }
        public Expression IfTrue { get; private set; }
        public Expression IfFalse { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            var condition = Condition.Evaluate(env.Local());
            var ifTrue = IfTrue.Evaluate(env.Local());
            var ifFalse = IfFalse.Evaluate(env.Local());

            return new ApplicationValue(
                new FunctionValue(cond => Defaults.True.CompareTo(cond.WHNFValue) == 0 ? ifTrue : ifFalse),
                condition
                );
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                var condition = Condition.Infer(env.Local(), subst);
                condition.Unify(TypeDefaults.Bool, subst);

                var ifTrue = IfTrue.Infer(env.Local(), subst);
                var ifFalse = IfFalse.Infer(env.Local(), subst);
                ifTrue.Unify(ifFalse, subst);

                return ifTrue.Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In an if expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return string.Format("if {0} then {1} else {2}", Condition, IfTrue, IfFalse);
        }
    }
}