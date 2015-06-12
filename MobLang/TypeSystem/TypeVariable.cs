using System.Collections.Generic;
using System.Text;
using MobLang.Exceptions;

namespace MobLang.TypeSystem
{
    internal class TypeVariable : Type
    {
        private static int _current;
        public readonly string Name;
        public string RigidName { get; private set; }

        public TypeVariable()
        {
            Name = "var" + (++_current);
        }

        public TypeVariable(string name) : this()
        {
            RigidName = "'" + name;
        }

        public override string ToString()
        {
            return RigidName ?? Name;
        }

        public override Type Perform(Substitution substitution)
        {
            if (!substitution.Map.ContainsKey(this))
                return this;

            if(RigidName != null)
                throw RigidTypeException.Create(RigidName, substitution.Map[this]);
                
            return substitution.Map[this].Perform(substitution);
        }

        public override Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict)
        {
            if(!dict.ContainsKey(this))
                dict.Add(this, RigidName == null ? this : new TypeVariable());
            return dict[this];
        }

        public override HashSet<TypeVariable> FreeVariables()
        {
            return new HashSet<TypeVariable> {this};
        }

        public override void UnifyWithRigid(TypeVariable var, Substitution substitution)
        {
            if (var == this)
                return;

            if (RigidName != null)
                throw RigidTypeException.Create(RigidName, var);

            substitution.Bind(this, var);
        }

        public override void DoUnify(Type var, Substitution substitution)
        {
            if (this == var)
                return;

            if (RigidName != null)
                var.UnifyWithRigid(this, substitution);
            else
            {
                if (var.FreeVariables().Contains(this))
                    throw OccursCheckException.Create(var.Perform(substitution), this.Perform(substitution));
                substitution.Bind(this, var);
            }
        }

        public override void SubstUnify(Type that, Substitution substitution)
        {
            if (that != this)
                that.DoUnify(this, substitution);
        }

        public override void PrettyPrint(TypePrettyPrinter typePrettyPrinter, StringBuilder stringBuilder, int level)
        {
            stringBuilder.Append(RigidName ?? typePrettyPrinter[this]);
        }
    }
}