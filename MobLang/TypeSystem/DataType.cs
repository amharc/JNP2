using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobLang.Exceptions;

namespace MobLang.TypeSystem
{
    internal class DataType : Type
    {
        public readonly List<Type> Arguments;
        public readonly string Name;

        public DataType(string name, List<Type> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public override string ToString()
        {
            if (Arguments.Count == 0)
                return Name;
            return Name + string.Join(" ", Arguments.Select(x => "(" + x + ")"));
        }

        public override Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict)
        {
            return new DataType(Name, Arguments.Select(x => x.RemoveRigids(dict)).ToList());
        }

        public override Type Perform(Substitution substitution)
        {
            return new DataType(Name, Arguments.Select(x => x.Perform(substitution)).ToList());
        }

        public override HashSet<TypeVariable> FreeVariables()
        {
            var res = new HashSet<TypeVariable>();
            foreach (var arg in Arguments)
                res.UnionWith(arg.FreeVariables());
            return res;
        }

        public override void DoUnify(DataType var, Substitution substitution)
        {
            if (Name != var.Name || Arguments.Count != var.Arguments.Count)
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
            if (level > 4 && Arguments.Count > 0)
                stringBuilder.Append('(');
            stringBuilder.Append(Name);
            foreach (var arg in Arguments)
            {
                stringBuilder.Append(' ');
                arg.PrettyPrint(typePrettyPrinter, stringBuilder, 5);
            }
            if (level > 4 && Arguments.Count > 0)
                stringBuilder.Append(')');
        }
    }
}