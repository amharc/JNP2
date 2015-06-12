using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal abstract class DefClause
    {
        public abstract void PrepareValueEnvironment(Environment env);
        public abstract void AddValueToEnvironment(Environment env);
        public abstract void PrepareTypeEnvironment(Environment env, Substitution subst);
        public abstract void AddTypeToEnvironment(Environment env, Substitution subst);
    }
}