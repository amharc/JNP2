using System;

namespace MobLang.Interpreter
{
    internal abstract class Value : IComparable<Value>
    {
        public abstract int CompareTo(Value that);
        public abstract WeakHeadNormalFormValue ToWHNF();
        public abstract WeakHeadNormalFormValue ToNormalForm();
    }
}