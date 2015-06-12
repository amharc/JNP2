using System;

namespace MobLang.Interpreter
{
    internal class ApplicationValue : Value
    {
        public readonly LazyValue Argument;
        public readonly LazyValue Function;

        public ApplicationValue(LazyValue function, LazyValue argument)
        {
            Function = function;
            Argument = argument;
        }

        public override WeakHeadNormalFormValue ToWHNF()
        {
            var function = (FunctionValue) Function.WHNFValue;
            return function.Function(Argument).WHNFValue;
        }

        public override WeakHeadNormalFormValue ToNormalForm()
        {
            return ToWHNF().ToNormalForm();
        }

        public override string ToString()
        {
            return string.Format("<apply {0} on {1}>", Function, Argument);
        }

        public override int CompareTo(Value that)
        {
            throw new NotImplementedException("Should not happen");
        }
    }
}