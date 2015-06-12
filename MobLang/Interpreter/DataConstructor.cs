using MobLang.TypeSystem;

namespace MobLang.Interpreter
{
    internal class DataConstructor
    {
        public readonly int Arity;
        public readonly string Name;
        public readonly Type ResultType;
        public readonly Type Type;
        // unary
        public DataConstructor(Type type, string name)
            : this(type, name, type, 0)
        {
        }

        public DataConstructor(Type type, string name, Type resultType, int arity)
        {
            Type = type;
            Name = name;
            ResultType = resultType;
            Arity = arity;
        }

        public override bool Equals(object obj)
        {
            var that = obj as DataConstructor;

            if (that == null)
                return false;

            return Name == that.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}