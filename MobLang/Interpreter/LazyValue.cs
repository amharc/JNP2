using MobLang.Exceptions;

namespace MobLang.Interpreter
{
    internal class LazyValue
    {
        private static uint _recursionDepth;
        private const uint MaxRecursionDepth = 1023;

        public LazyValue(Value value)
        {
            Value = value;
        }

        public Value Value { get; set; }

        private static void EnterFrame()
        {
            if(++_recursionDepth >= MaxRecursionDepth)
                throw new SafeStackOverflowException();
            if (Program.OnRecursion != null)
                Program.OnRecursion();
        }

        private static void ExitFrame()
        {
            --_recursionDepth;
        }

        public WeakHeadNormalFormValue WHNFValue
        {
            get
            {
                try
                {
                    EnterFrame();
                    Value = Value.ToWHNF();
                }
                finally
                {
                    ExitFrame();
                }
                return (WeakHeadNormalFormValue) Value;
            }
        }

        public WeakHeadNormalFormValue NormalFormValue
        {
            get
            {
                try
                {
                    EnterFrame();
                    Value = Value.ToNormalForm();
                }
                finally
                {
                    ExitFrame();
                }
                return (WeakHeadNormalFormValue) Value;
            }
        }

        public static implicit operator LazyValue(Value value)
        {
            return new LazyValue(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}