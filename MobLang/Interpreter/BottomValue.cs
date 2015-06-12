using MobLang.Exceptions;

namespace MobLang.Interpreter
{
    internal class BottomValue : Value
    {
        public override WeakHeadNormalFormValue ToWHNF()
        {
            throw new BottomException();
        }

        public override WeakHeadNormalFormValue ToNormalForm()
        {
            throw new BottomException();
        }

        public override int CompareTo(Value that)
        {
            throw new BottomException();
        }
    }
}