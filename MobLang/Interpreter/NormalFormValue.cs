namespace MobLang.Interpreter
{
    internal abstract class NormalFormValue : WeakHeadNormalFormValue
    {
        public override sealed WeakHeadNormalFormValue ToNormalForm()
        {
            return this;
        }

        public abstract override int CompareTo(Value that);
    }
}