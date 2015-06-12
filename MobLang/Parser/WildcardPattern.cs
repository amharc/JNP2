using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class WildcardPattern : Pattern
    {
        public override bool Match(LazyValue value, ref Environment env)
        {
            return true;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            return new TypeVariable();
        }

        public override string ToString()
        {
            return "_";
        }
    }
}