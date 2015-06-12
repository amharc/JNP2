using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobLang.Exceptions;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;

namespace MobLang.Parser
{
    class TypeAnnotatedDefFunctionClause : DefFunctionClause
    {
        public readonly TypeClause AnnotatedType;

        public TypeAnnotatedDefFunctionClause(string name, List<Pattern> arguments, Expression body, TypeClause type) : base(name, arguments, body)
        {
            AnnotatedType = type;
        }

        public override void PrepareTypeEnvironment(Environment env, Substitution subst)
        {
            Type = AnnotatedType.Parse(env, subst, true);
            Type = Type.Generalize(env);
            env.Bind(Name, Type);
        }

        public override void AddTypeToEnvironment(Environment env, Substitution subst)
        {
            try
            {
                UpdateType(env, subst, true);
                env.Bind(Name, Type);
            }
            catch (StackedException ex)
            {
                var sb = new StringBuilder("In a def clause: " + this + " with a rigid type: ");
                Type.PrettyPrint(ex.PrettyPrinter, sb, 0);
                ex.Push(sb.ToString());
                throw;
            }
        }
    }
}
