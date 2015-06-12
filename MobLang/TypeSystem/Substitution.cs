using System.Collections.Immutable;

namespace MobLang.TypeSystem
{
    internal class Substitution
    {
        public ImmutableDictionary<TypeVariable, Type> Map;

        public Substitution(ImmutableDictionary<TypeVariable, Type> map)
        {
            Map = map;
        }

        public Substitution() : this(ImmutableDictionary.Create<TypeVariable, Type>())
        {
        }

        public Substitution Local()
        {
            return new Substitution(Map);
        }

        public void Bind(TypeVariable variable, Type type)
        {
            Map = Map.Add(variable, type);
        }
    }
}