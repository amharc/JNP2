using System.Collections.Generic;

namespace MobLang.TypeSystem
{
    internal static class TypeDefaults
    {
        public static DataType Bool = new DataType("Bool", new List<Type>());
        public static TupleType Unit = new TupleType(new List<Type>());
    }
}