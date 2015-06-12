using System;
using System.Text;
using MobLang.TypeSystem;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Exceptions
{
    public class OccursCheckException : StackedException
    {
        internal OccursCheckException(string type, string name, TypePrettyPrinter printer)
            : base(string.Format("Occurs check: {1} occurs in {0}", type, name), printer)
        {
        }

        internal static OccursCheckException Create(Type lhs, Type rhs)
        {
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var tpp = new TypePrettyPrinter();
            lhs.PrettyPrint(tpp, sb1, 0);
            rhs.PrettyPrint(tpp, sb2, 0);
            return new OccursCheckException(sb1.ToString(), sb2.ToString(), tpp);
        }
    }
}