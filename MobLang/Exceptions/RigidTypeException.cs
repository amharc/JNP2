using System;
using System.Text;
using MobLang.TypeSystem;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Exceptions
{
    public class RigidTypeException : StackedException
    {
        internal RigidTypeException(string name, string type, TypePrettyPrinter printer)
            : base(string.Format("Could not unify a rigid type variable {0} with {1}", name, type), printer)
        {
        }

        internal static RigidTypeException Create(string name, Type type)
        {
            var sb = new StringBuilder();
            var tpp = new TypePrettyPrinter();
            type.PrettyPrint(tpp, sb, 0);
            return new RigidTypeException(name, sb.ToString(), tpp);
        }
    }
}
