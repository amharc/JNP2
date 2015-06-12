using System;
using System.Collections.Generic;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.TypeSystem;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang.Parser
{
    internal class DefExpression : Expression
    {
        public DefExpression(List<DefClause> clauses)
        {
            Clauses = clauses;
        }

        public List<DefClause> Clauses { get; private set; }

        protected void AddToEnvironment(Environment env)
        {
            foreach (var clause in Clauses)
                clause.PrepareValueEnvironment(env);
            foreach (var clause in Clauses)
                clause.AddValueToEnvironment(env);
        }

        public override LazyValue Evaluate(Environment env)
        {
            AddToEnvironment(env);
            return Defaults.Unit;
        }

        public override Type Infer(Environment env, Substitution subst)
        {
            try
            {
                foreach (var clause in Clauses)
                    clause.PrepareTypeEnvironment(env, subst);
                foreach (var clause in Clauses)
                    clause.AddTypeToEnvironment(env, subst);

                return TypeDefaults.Unit;
            }
            catch (StackedException ex)
            {
                ex.Push("In a def-expression: " + this);
                throw;
            }
        }

        public override string ToString()
        {
            return "def " + string.Join(" and ", Clauses);
        }
    }
}