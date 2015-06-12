using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal abstract class Pattern
    {
        public abstract bool Match(LazyValue value, ref Environment env);
        public abstract Type Infer(Environment env, Substitution subst);
    }
}