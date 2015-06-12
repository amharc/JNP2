using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobLang.Exceptions;
using MobLang.Interpreter;

namespace MobLang.TypeSystem
{
    internal abstract class Type
    {
        public abstract Type Perform(Substitution substitution);

        public virtual Type Instantiate()
        {
            return RemoveRigids();
        }

        public string PrettyPrint()
        {
            var sb = new StringBuilder();
            PrettyPrint(new TypePrettyPrinter(), sb, 0);
            return sb.ToString();
        }

        public abstract void PrettyPrint(TypePrettyPrinter typePrettyPrinter, StringBuilder stringBuilder, int level);

        public Type Generalize(Environment env)
        {
            var free = FreeVariables();

            foreach (var other in env.ValueTypes.Values.SelectMany(binding => binding.FreeVariables()))
                free.Remove(other);

            return free.Count > 0 ? new TypeScheme(free, this) : this;
        }

        public Type RemoveRigids()
        {
            return RemoveRigids(new Dictionary<TypeVariable, TypeVariable>());
        }
        public abstract Type RemoveRigids(Dictionary<TypeVariable, TypeVariable> dict);

        public abstract HashSet<TypeVariable> FreeVariables();

        public void Unify(Type that, Substitution substitution)
        {
            try
            {
                Perform(substitution).SubstUnify(that.Perform(substitution), substitution);
            }
            catch (StackedException ex)
            {
                var sb = new StringBuilder("While trying to unify ");
                PrettyPrint(ex.PrettyPrinter, sb, 0);
                sb.Append(" with ");
                that.PrettyPrint(ex.PrettyPrinter, sb, 0);
                ex.Push(sb.ToString());
                throw;
            }
        }

        public virtual void SubstUnify(Type that, Substitution substitution)
        {
            if (this != that)
                that.DoUnify(this, substitution);
        }

        public virtual void DoUnify(Type that, Substitution substitution)
        {
            throw TypeException.Create(Perform(substitution), that.Perform(substitution));
        }

        public virtual void UnifyWithRigid(TypeVariable var, Substitution subst)
        {
            throw RigidTypeException.Create(var.RigidName, this);
        }

        public virtual void DoUnify(TypeVariable var, Substitution substitution)
        {
            var.DoUnify(this, substitution);
        }

        public virtual void DoUnify(IntegralType var, Substitution substitution)
        {
            DoUnify((Type) var, substitution);
        }

        public virtual void DoUnify(CharacterType var, Substitution substitution)
        {
            DoUnify((Type) var, substitution);
        }

        public virtual void DoUnify(FloatingType var, Substitution substitution)
        {
            DoUnify((Type) var, substitution);
        }

        public virtual void DoUnify(FunctionType var, Substitution substitution)
        {
            DoUnify((Type) var, substitution);
        }

        public virtual void DoUnify(DataType var, Substitution substitution)
        {
            DoUnify((Type) var, substitution);
        }

        public virtual void DoUnify(TupleType var, Substitution substitution)
        {
            DoUnify((Type) var, substitution);
        }
    }
}