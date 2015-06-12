using System.Collections.Generic;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;

namespace MobLang.Parser
{
    internal class DataTypeDeclarationExpression : Expression
    {
        public readonly List<string> Arguments;
        public readonly List<DataTypeConstructorDeclaration> Constructors;
        public readonly string Name;

        public DataTypeDeclarationExpression(string name, List<string> arguments,
            List<DataTypeConstructorDeclaration> constructors)
        {
            Name = name;
            Arguments = arguments;
            Constructors = constructors;
        }

        public override LazyValue Evaluate(Environment env)
        {
            return Defaults.Unit;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                if (env.Types.ContainsKey(Name))
                    throw new DataTypeException(string.Format("Type {0} is already defined", Name));

                var argVariables = (from arg in Arguments select new TypeVariable()).ToList<Type>();

                var type = new DataType(Name, argVariables);
                env.BindType(Name, type.Generalize(env));

                var local = env.Local();

                for (var i = 0; i < Arguments.Count; ++i)
                    local.BindType(Arguments[i], argVariables[i]);

                foreach (var decl in Constructors)
                    decl.Parse(env, local, type, subst);

                return new TupleType(new List<Type>());
            }
            catch (StackedException ex)
            {
                ex.Push("In a data type declaration: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            var hdr = "data " + Name;
            if (Arguments.Count > 0)
                hdr = hdr + " " + string.Join(" ", Arguments);
            return hdr + string.Join(" ", Constructors);
        }
    }
}