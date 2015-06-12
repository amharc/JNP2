using System.Collections.Generic;
using MobLang.TypeSystem;

namespace MobLang.Interpreter
{
    internal static class Defaults
    {
        public static readonly TupleValue Unit = new TupleValue();
        public static readonly BottomValue Bottom = new BottomValue();
        public static readonly DataConstructor TrueConstructor = new DataConstructor(TypeDefaults.Bool, "True");
        public static readonly DataConstructor FalseConstructor = new DataConstructor(TypeDefaults.Bool, "False");
        public static readonly DataValue True = new DataValue(TrueConstructor, new List<LazyValue>());
        public static readonly DataValue False = new DataValue(FalseConstructor, new List<LazyValue>());
    }
}