using System.Collections.Generic;
using System.Text;

namespace MobLang.TypeSystem
{
    internal class FunctionType : Type
    {
        public readonly Type Argument;
        public readonly Type Result;

        public FunctionType(Type argument, Type result)
        {
            Argument = argument;
            Result = result;
        }

        public override Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict)
        {
            return new FunctionType(Argument.RemoveRigids(dict), Result.RemoveRigids(dict));
        }

        public override string ToString()
        {
            return string.Format("({0} -> {1})", Argument, Result);
        }

        public override Type Perform(Substitution substitution)
        {
            return new FunctionType(Argument.Perform(substitution), Result.Perform(substitution));
        }

        public override HashSet<TypeVariable> FreeVariables()
        {
            var res = Argument.FreeVariables();
            res.UnionWith(Result.FreeVariables());
            return res;
        }

        public override void DoUnify(FunctionType var, Substitution substitution)
        {
            Argument.Perform(substitution).Unify(var.Argument.Perform(substitution), substitution);
            Result.Perform(substitution).Unify(var.Result.Perform(substitution), substitution);
        }

        public override void SubstUnify(Type that, Substitution substitution)
        {
            that.DoUnify(this, substitution);
        }

        public override void PrettyPrint(TypePrettyPrinter typePrettyPrinter, StringBuilder stringBuilder, int level)
        {
            if (level > 2)
                stringBuilder.Append('(');
            Argument.PrettyPrint(typePrettyPrinter, stringBuilder, 3);
            stringBuilder.Append(" -> ");
            Result.PrettyPrint(typePrettyPrinter, stringBuilder, 2);
            if (level > 2)
                stringBuilder.Append(')');
        }
    }
}