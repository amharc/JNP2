using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class NamePattern : Pattern
    {
        public readonly string Name;

        public NamePattern(string name)
        {
            Name = name;
        }

        public override bool Match(LazyValue value, ref Environment env)
        {
            env.Bind(Name, value);
            return true;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            Type self = new TypeVariable();
            env.Bind(Name, self);
            return self;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}