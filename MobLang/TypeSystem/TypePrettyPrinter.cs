using System.Collections.Generic;
using System.Text;

namespace MobLang.TypeSystem
{
    internal class TypePrettyPrinter
    {
        private readonly StringBuilder _name;
        private readonly Dictionary<TypeVariable, string> _saved;

        public TypePrettyPrinter()
        {
            _name = new StringBuilder();
            _saved = new Dictionary<TypeVariable, string>();
        }

        public string this[TypeVariable variable]
        {
            get
            {
                if (!_saved.ContainsKey(variable))
                    _saved.Add(variable, Next());

                return _saved[variable];
            }
        }

        private string Next()
        {
            int i;
            for (i = _name.Length - 1; i >= 0 && _name[i] == 'z'; --i)
                _name[i] = 'a';

            if (i >= 0)
                _name[i]++;
            else
                _name.Append('a');

            return _name.ToString();
        }
    }
}