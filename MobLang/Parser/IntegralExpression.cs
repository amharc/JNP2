using System.Numerics;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class IntegralExpression : Expression
    {
        public IntegralExpression(BigInteger value)
        {
            Value = value;
        }

        public BigInteger Value { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            return new IntegralValue(Value);
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            return new IntegralType();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}