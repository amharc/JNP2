using System.Collections.Generic;
using System.Text;

namespace MobLang.TypeSystem
{
    internal class FloatingType : Type
    {
        public override string ToString()
        {
            return "Float";
        }

        public override Type Perform(Substitution substitution)
        {
            return this;
        }

        public override Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict)
        {
            return this;
        }

        public override HashSet<TypeVariable> FreeVariables()
        {
            return new HashSet<TypeVariable>();
        }

        public override void DoUnify(FloatingType var, Substitution substitution)
        {
        }

        public override void SubstUnify(Type that, Substitution substitution)
        {
            that.DoUnify(this, substitution);
        }

        public override void PrettyPrint(TypePrettyPrinter typePrettyPrinter, StringBuilder stringBuilder, int level)
        {
            stringBuilder.Append("Float");
        }
    }
}