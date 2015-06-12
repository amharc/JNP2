using System;
using System.Collections.Generic;
using System.Linq;

namespace MobLang.Interpreter
{
    internal class DataValue : WeakHeadNormalFormValue
    {
        public readonly List<LazyValue> Arguments;
        public readonly DataConstructor Constructor;

        public DataValue(DataConstructor constructor, List<LazyValue> arguments)
        {
            Constructor = constructor;
            Arguments = arguments;
        }

        public override string ToString()
        {
            switch(Constructor.Name)
            {
                case "Nil":
                    return "[]";
                case "Cons":
                    var elements = new List<string>();
                    DataValue cur = this;
                    while (cur.Constructor.Name == "Cons")
                    {
                        elements.Add(cur.Arguments[0].ToString());
                        cur = (DataValue) cur.Arguments[1].WHNFValue;
                    }
                    return "[" + string.Join(", ", elements) + "]";
                default:
                    return Arguments.Aggregate(Constructor.ToString(), (x, y) => x + " (" + y + ")");
            }
        }

        public override WeakHeadNormalFormValue ToNormalForm()
        {
            foreach (var arg in Arguments)
                arg.Value = arg.Value.ToNormalForm();
            return this;
        }

        public override int CompareTo(Value that)
        {
            var other = (DataValue) that;

            var ret = string.Compare(Constructor.Name, other.Constructor.Name, StringComparison.Ordinal);
            if (ret != 0)
                return ret;

            for (var i = 0; i < Arguments.Count; ++i)
            {
                ret = Arguments[i].WHNFValue.CompareTo(other.Arguments[i].WHNFValue);
                if (ret != 0)
                    return ret;
            }

            return 0;
        }
    }
}