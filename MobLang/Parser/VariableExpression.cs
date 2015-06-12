using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class VariableExpression : Expression
    {
        public VariableExpression(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            return env.Values[Name];
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                if (!env.ValueTypes.ContainsKey(Name))
                    throw new UnboundNameException(Name);

                return env.ValueTypes[Name].RemoveRigids().Instantiate().Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In a name use: " + Name);
                throw;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}