using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobLang.TypeSystem
{
    internal class TypeScheme : Type
    {
        public readonly HashSet<TypeVariable> Arguments;
        public readonly Type Inner;

        public TypeScheme(HashSet<TypeVariable> arguments, Type inner)
        {
            Arguments = arguments;
            Inner = inner;
        }

        public override Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict)
        {
            var inner = Inner.RemoveRigids(dict);
            // C# lacks return value contravariance (unlike Java), so an explicit cast is needed
            return new TypeScheme(
                new HashSet<TypeVariable>(Arguments.Select(x => (TypeVariable)x.RemoveRigids(dict))),
                inner
            );
        }

        public override HashSet<TypeVariable> FreeVariables()
        {
            var res = Inner.FreeVariables();
            foreach (var arg in Arguments)
                res.Remove(arg);
            return res;
        }

        public override Type Instantiate()
        {
            var subst = new Substitution();
            foreach (var arg in Arguments.Where(arg => arg.RigidName == null))
                subst.Bind(arg, new TypeVariable());
            return Inner.Perform(subst);
        }

        public override Type Perform(Substitution substitution)
        {
            var local = substitution.Local();
            foreach (var arg in Arguments)
                local.Map = local.Map.Remove(arg);
            return new TypeScheme(Arguments, Inner.Perform(local));
        }

        public override string ToString()
        {
            return string.Format("forall {0}. {1}", string.Join(" ", Arguments), Inner);
        }

        public override void PrettyPrint(TypePrettyPrinter typePrettyPrinter, StringBuilder stringBuilder, int level)
        {
            stringBuilder.Append("forall");
            foreach (var variable in Arguments)
                stringBuilder.Append(" " + typePrettyPrinter[variable]);
            stringBuilder.Append(". ");
            Inner.PrettyPrint(typePrettyPrinter, stringBuilder, 1);
        }
    }
}