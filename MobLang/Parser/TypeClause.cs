using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal abstract class TypeClause
    {
        public abstract Type Parse(Environment env, Substitution subst, bool allowUnbound = false);
    }
}