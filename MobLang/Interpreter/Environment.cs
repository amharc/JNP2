using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using MobLang.TypeSystem;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Interpreter
{
    internal class Environment
    {
        public Environment()
        {
            {
                var builder = ImmutableDictionary.CreateBuilder<string, LazyValue>();
                builder.Add("+", BuiltinFunction(BigInteger.Add));
                builder.Add("-", BuiltinFunction(BigInteger.Subtract));
                builder.Add("*", BuiltinFunction(BigInteger.Multiply));
                builder.Add("/", BuiltinFunction(BigInteger.Divide));
                builder.Add("%", BuiltinFunction(BigInteger.Remainder));
                builder.Add("negate", BuiltinFunction(BigInteger.Negate));
                builder.Add("powmod", BuiltinFunction(BigInteger.ModPow));

                builder.Add("==", BuiltinFunction((x, y) => x.CompareTo(y) == 0));
                builder.Add("!=", BuiltinFunction((x, y) => x.CompareTo(y) != 0));
                builder.Add(">", BuiltinFunction((x, y) => x.CompareTo(y) > 0));
                builder.Add(">=", BuiltinFunction((x, y) => x.CompareTo(y) >= 0));
                builder.Add("<", BuiltinFunction((x, y) => x.CompareTo(y) < 0));
                builder.Add("<=", BuiltinFunction((x, y) => x.CompareTo(y) <= 0));

                builder.Add("True", Defaults.True);
                builder.Add("False", Defaults.False);
                Values = builder.ToImmutableDictionary();
            }

            {
                var builder = ImmutableDictionary.CreateBuilder<string, Type>();
                var unaryInt = new FunctionType(new IntegralType(), new IntegralType());
                var binaryInt = new FunctionType(new IntegralType(), unaryInt);
                var ternaryInt = new FunctionType(new IntegralType(), binaryInt);

                var gen = new TypeVariable();
                var binaryGen = new TypeScheme(new HashSet<TypeVariable> {gen},
                    new FunctionType(gen, new FunctionType(gen, TypeDefaults.Bool)));

                builder.Add("+", binaryInt);
                builder.Add("-", binaryInt);
                builder.Add("*", binaryInt);
                builder.Add("/", binaryInt);
                builder.Add("%", binaryInt);
                builder.Add("negate", unaryInt);
                builder.Add("powmod", ternaryInt);

                builder.Add("==", binaryGen);
                builder.Add("!=", binaryGen);
                builder.Add(">", binaryGen);
                builder.Add(">=", binaryGen);
                builder.Add("<", binaryGen);
                builder.Add("<=", binaryGen);

                builder.Add("True", TypeDefaults.Bool);
                builder.Add("False", TypeDefaults.Bool);
                ValueTypes = builder.ToImmutableDictionary();
            }

            {
                var builder = ImmutableDictionary.CreateBuilder<string, DataConstructor>();
                builder.Add("True", Defaults.TrueConstructor);
                builder.Add("False", Defaults.FalseConstructor);
                Constructors = builder.ToImmutableDictionary();
            }

            {
                var builder = ImmutableDictionary.CreateBuilder<string, Type>();
                builder.Add("Int", new IntegralType());
                builder.Add("Float", new FloatingType());
                builder.Add("Bool", TypeDefaults.Bool);
                Types = builder.ToImmutableDictionary();
            }
        }

        public Environment(ImmutableDictionary<string, LazyValue> values,
            ImmutableDictionary<string, Type> valueTypes,
            ImmutableDictionary<string, DataConstructor> constructors,
            ImmutableDictionary<string, Type> types)
        {
            Values = values;
            ValueTypes = valueTypes;
            Constructors = constructors;
            Types = types;
        }

        public ImmutableDictionary<string, LazyValue> Values { get; set; }
        public ImmutableDictionary<string, Type> ValueTypes { get; set; }
        public ImmutableDictionary<string, DataConstructor> Constructors { get; set; }
        public ImmutableDictionary<string, Type> Types { get; set; }

        private static LazyValue BuiltinFunction(Func<BigInteger, BigInteger, BigInteger> op)
        {
            return new FunctionValue(x => new FunctionValue(y =>
            {
                var forcedX = (IntegralValue) x.WHNFValue;
                var forcedY = (IntegralValue) y.WHNFValue;

                return new IntegralValue(op(forcedX.Value, forcedY.Value));
            }));
        }

        private static LazyValue BuiltinFunction(Func<BigInteger, BigInteger> op)
        {
            return new FunctionValue(x =>
            {
                var forcedX = (IntegralValue) x.WHNFValue;

                return new IntegralValue(op(forcedX.Value));
            });
        }

        private static LazyValue BuiltinFunction(Func<BigInteger, BigInteger, BigInteger, BigInteger> op)
        {
            return new FunctionValue(x => new FunctionValue(y => new FunctionValue(z =>
            {
                var forcedX = (IntegralValue) x.WHNFValue;
                var forcedY = (IntegralValue) y.WHNFValue;
                var forcedZ = (IntegralValue) z.WHNFValue;

                return new IntegralValue(op(forcedX.Value, forcedY.Value, forcedZ.Value));
            })));
        }

        private static LazyValue BuiltinFunction(Func<Value, Value, bool> op)
        {
            return new FunctionValue(x => new FunctionValue(y =>
            {
                if (op(x.WHNFValue, y.WHNFValue))
                    return Defaults.True;
                return Defaults.False;
            }));
        }

        public Environment Local()
        {
            return new Environment(Values, ValueTypes, Constructors, Types);
        }

        public void Bind(string name, LazyValue value)
        {
            Values = Values.SetItem(name, value);
        }

        public void Bind(string name, Type type)
        {
            ValueTypes = ValueTypes.SetItem(name, type);
        }

        public void Perform(Substitution subst)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, Type>();

            foreach (var binding in ValueTypes)
                builder.Add(binding.Key, binding.Value.Perform(subst));

            ValueTypes = builder.ToImmutableDictionary();
        }

        public void Bind(string name, DataConstructor constructor)
        {
            Constructors = Constructors.SetItem(name, constructor);
        }

        public void BindType(string name, Type type)
        {
            Types = Types.SetItem(name, type);
        }
    }
}