using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class ApplicationExpression : Expression
    {
        public ApplicationExpression(Expression function, Expression argument)
        {
            Function = function;
            Argument = argument;
        }

        public Expression Function { get; private set; }
        public Expression Argument { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            var function = Function.Evaluate(env.Local());
            var argument = Argument.Evaluate(env.Local());
            return new ApplicationValue(function, argument);
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                var function = Function.Infer(env.Local(), subst);
                var argument = Argument.Infer(env.Local(), subst);
                var result = new TypeVariable();
                function.Unify(new FunctionType(argument, result), subst);
                return result.Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In an application expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}) ({1})", Function, Argument);
        }
    }
}