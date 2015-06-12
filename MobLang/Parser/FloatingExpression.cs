using System;
using MobLang.Interpreter;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    internal class FloatingExpression : Expression
    {
        public FloatingExpression(double value)
        {
            Value = value;
        }

        public double Value { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            throw new NotImplementedException();
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            return new FloatingType();
        }
    }
}