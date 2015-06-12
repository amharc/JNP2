using System;
using MobLang.Interpreter;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    internal abstract class Expression
    {
        public abstract LazyValue Evaluate(Environment env);

        public abstract Type Infer(Environment env, Substitution subst);

        public Type Infer(Environment env)
        {
            var subst = new Substitution();
            var ret = Infer(env, subst);
            ret.Perform(subst);
            env.Perform(subst);
            return ret;
        }
    }
}