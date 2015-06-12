using System;
using MobLang.Interpreter;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    internal class CharacterExpression : Expression
    {
        public CharacterExpression(char value)
        {
            Value = value;
        }

        public char Value { get; private set; }

        public override LazyValue Evaluate(Environment env)
        {
            return new CharacterValue(Value);
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            return new CharacterType();
        }

        public override string ToString()
        {
            return String.Format("'{0}'", Value);
        }
    }
}