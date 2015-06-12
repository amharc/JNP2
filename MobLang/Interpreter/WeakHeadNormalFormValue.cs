namespace MobLang.Interpreter
{
    internal abstract class WeakHeadNormalFormValue : Value
    {
        public override sealed WeakHeadNormalFormValue ToWHNF()
        {
            return this;
        }

        public abstract override int CompareTo(Value that);
    }
}