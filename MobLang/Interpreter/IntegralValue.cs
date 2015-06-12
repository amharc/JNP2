using System.Numerics;

namespace MobLang.Interpreter
{
    internal class IntegralValue : NormalFormValue
    {
        public readonly BigInteger Value;

        public IntegralValue(BigInteger value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int CompareTo(Value that)
        {
            return Value.CompareTo(((IntegralValue) that).Value);
        }
    }
}