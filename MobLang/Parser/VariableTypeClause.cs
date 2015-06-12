using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class VariableTypeClause : TypeClause
    {
        public readonly string Name;

        public VariableTypeClause(string name)
        {
            Name = name;
        }

        public override Type Parse(Environment env, Substitution subst, bool allowUnbound = false)
        {
            if (env.Types.ContainsKey(Name))
                return env.Types[Name];

            if (!allowUnbound)
                throw new DataTypeException(string.Format("Unbound type: {0}", Name));

            env.BindType(Name, new TypeVariable(Name));
            return env.Types[Name];
        }

        public override string ToString()
        {
            return Name;
        }
    }
}