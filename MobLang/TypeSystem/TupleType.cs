using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobLang.Exceptions;

namespace MobLang.TypeSystem
{
    internal class TupleType : Type
    {
        public readonly List<Type> Arguments;

        public TupleType(List<Type> arguments)
        {
            Arguments = arguments;
        }

        public override Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict)
        {
            return new TupleType(Arguments.Select(x => x.RemoveRigids(dict)).ToList());
        }

        public override string ToString()
        {
            return "(" + string.Join(", ", Arguments) + ")";
        }

        public override Type Perform(Substitution substitution)
        {
            return new TupleType(Arguments.Select(x => x.Perform(substitution)).ToList());
        }

        public override HashSet<TypeVariable> FreeVariables()
        {
            var res = new HashSet<TypeVariable>();
            foreach (var arg in Arguments)
                res.UnionWith(arg.FreeVariables());
            return res;
        }

        public override void DoUnify(TupleType var, Substitution substitution)
        {
            if (Arguments.Count != var.Arguments.Count)
                throw TypeException.Create(this.Perform(substitution), var.Perform(substitution));

            for (var i = 0; i < Arguments.Count; ++i)
                Arguments[i].Perform(substitution).Unify(var.Arguments[i].Perform(substitution), substitution);
        }

        public override void SubstUnify(Type that, Substitution substitution)
        {
            that.DoUnify(this, substitution);
        }

        public override void PrettyPrint(TypePrettyPrinter typePrettyPrinter, StringBuilder stringBuilder, int level)
        {
            stringBuilder.Append('(');

            if (Arguments.Count != 0)
            {
                Arguments[0].PrettyPrint(typePrettyPrinter, stringBuilder, 1);
                foreach (var arg in Arguments.Skip(1))
                {
                    stringBuilder.Append(", ");
                    arg.PrettyPrint(typePrettyPrinter, stringBuilder, 1);
                }
            }

            stringBuilder.Append(')');
        }
    }
}