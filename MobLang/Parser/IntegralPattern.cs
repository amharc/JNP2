using System.Numerics;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class IntegralPattern : Pattern
    {
        public readonly BigInteger Value;

        public IntegralPattern(BigInteger value)
        {
            Value = value;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            return new IntegralType();
        }

        public override bool Match(LazyValue value, ref Environment env)
        {
            return ((IntegralValue) value.WHNFValue).Value == Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}