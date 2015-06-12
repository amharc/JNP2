namespace MobLang.Interpreter
{
    internal class CharacterValue : NormalFormValue
    {
        public readonly char Value;

        public CharacterValue(char value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("'{0}'", Value);
        }

        public override int CompareTo(Value that)
        {
            return Value.CompareTo(((CharacterValue) that).Value);
        }
    }
}