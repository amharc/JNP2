using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    class TypeAnnotatedExpression : Expression
    {
        public readonly TypeClause Type;
        public readonly Expression Expression;

        public TypeAnnotatedExpression(Expression expression, TypeClause type)
        {
            Type = type;
            Expression = expression;
        }

        public override LazyValue Evaluate(Environment env)
        {
            return Expression.Evaluate(env);
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                var local = env.Local();
                var annotated = Type.Parse(local, subst, true);
                var inferred = Expression.Infer(local, subst);
                inferred.Unify(annotated, subst);
                return inferred.Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In an annotated expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return Expression + " : " + Type;
        }
    }
}
