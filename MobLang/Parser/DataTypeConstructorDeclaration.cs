using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    internal class DataTypeConstructorDeclaration
    {
        public readonly List<TypeClause> Arguments;
        public readonly string Name;

        public DataTypeConstructorDeclaration(string name, List<TypeClause> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public void Parse(Environment outer, Environment env, Type result, Substitution subst)
        {
            try
            {
                var fullType = Arguments.Select(arg => arg.Parse(env, subst)).Reverse().Aggregate(
                    result,
                    (res, cur) => new FunctionType(cur, res)
                    ).Perform(subst).Generalize(outer);

                var constructor = new DataConstructor(fullType, Name, result, Arguments.Count);

                Func<ImmutableList<LazyValue>, LazyValue> function =
                    arguments => new DataValue(constructor, arguments.ToList());

                for (var i = 0; i < Arguments.Count; ++i)
                {
                    var backup = function;
                    function = arguments => new FunctionValue(arg => backup(arguments.Add(arg)));
                }

                outer.Bind(Name, constructor);
                outer.Bind(Name, fullType);
                outer.Bind(Name, function(ImmutableList.Create(new LazyValue[] {})));
            }
            catch (StackedException ex)
            {
                ex.Push("In a data type constructor declaration: " + ex);
                throw;
            }
        }

        public override string ToString()
        {
            return Arguments.Count == 0 ? 
                string.Format("case {0}", Name) : 
                string.Format("case {0} of {1}", Name, string.Join(" ", Arguments.Select(x => string.Format("({0})", x))));
        }
    }
}