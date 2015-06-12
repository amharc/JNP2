using MobLang.Exceptions;

namespace MobLang.Interpreter
{
    internal class FunctionValue : NormalFormValue
    {
        public delegate LazyValue FunctionType(LazyValue argument);

        public readonly FunctionType Function;

        public FunctionValue(FunctionType function)
        {
            Function = function;
        }

        public override string ToString()
        {
            return "<function>";
        }

        public override int CompareTo(Value that)
        {
            throw new FunctionValueComparisonException();
        }
    }
}