using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    class TypeAnnotatedPattern : Pattern
    {
        public readonly TypeClause Type;
        public readonly Pattern Body;

        public TypeAnnotatedPattern(Pattern body, TypeClause type)
        {
            Type = type;
            Body = body;
        }

        public override bool Match(LazyValue value, ref Environment env)
        {
            return Body.Match(value, ref env);
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                var annotated = Type.Parse(env, subst, true);
                var inferred = Body.Infer(env, subst);
                annotated.Unify(inferred, subst);
                return annotated.Perform(subst);
            }
            catch (StackedException ex)
            {
                ex.Push("In an annotated pattern: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return Body + " : " + Type;
        }
    }
}
