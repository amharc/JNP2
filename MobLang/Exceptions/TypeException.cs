using System;
using System.Text;
using MobLang.TypeSystem;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Exceptions
{
    public class TypeException : StackedException
    {
        internal TypeException(string lhs, string rhs, TypePrettyPrinter printer)
            : base(string.Format("Could not unify {0} with {1}", lhs, rhs), printer)
        {
        }

        internal static TypeException Create(Type lhs, Type rhs)
        {
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var tpp = new TypePrettyPrinter();
            lhs.PrettyPrint(tpp, sb1, 0);
            rhs.PrettyPrint(tpp, sb2, 0);
            return new TypeException(sb1.ToString(), sb2.ToString(), tpp);
        }
    }
}