using System.Collections.Generic;
using System.Linq;

namespace MobLang.Interpreter
{
    internal class TupleValue : WeakHeadNormalFormValue
    {
        public readonly List<LazyValue> List;

        public TupleValue() : this(new List<LazyValue>())
        {
        }

        public TupleValue(List<LazyValue> list)
        {
            List = list;
        }

        public override string ToString()
        {
            return "(" + string.Join(", ", List) + ")";
        }

        public override WeakHeadNormalFormValue ToNormalForm()
        {
            foreach (var arg in List)
                arg.Value = arg.Value.ToNormalForm();
            return this;
        }

        public override int CompareTo(Value that)
        {
            var other = (TupleValue) that;

            return List.Select((t, i) => t.WHNFValue.CompareTo(other.List[i].WHNFValue)).FirstOrDefault(ret => ret != 0);
        }
    }
}