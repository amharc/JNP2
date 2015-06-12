using System;
using System.Text;
using MobLang.Exceptions;
using MobLang.Interpreter;
using MobLang.Properties;
using Environment = MobLang.Interpreter.Environment;
using Type = MobLang.TypeSystem.Type;

namespace MobLang
{
    public class Program
    {
        private Environment _env;
        private const string TypeOfCommand = "#type ";

        public delegate void WriterDelegate(string line);

        public WriterDelegate Writer;
        public WriterDelegate ErrorWriter;

        public delegate void RecursionDelegate();

        public static RecursionDelegate OnRecursion;

        public Program()
        {
            _env = new Environment();
        }

        private void Write(string line)
        {
            if (Writer != null)
                Writer(line);
        }

        private void ErrorWrite(string line)
        {
            if (ErrorWriter != null)
                ErrorWriter(line);
        }

        private Tuple<Type, LazyValue> Evaluate(string input)
        {
            var parser = new Parser.Parser(input);
            var expression = parser.Result;
            var local = _env.Local();
            var type = expression.Infer(local);
            var value = expression.Evaluate(local);
            _env = local;
            return Tuple.Create(type, value);
        }

        public void UnsafeExecute(string input, bool interactive = false)
        {
            var tuple = Evaluate(input);
            var type = tuple.Item1;
            var value = tuple.Item2.NormalFormValue;
            if (interactive)
                Write(string.Format(Resources.Program_Execute_Result, value, type.PrettyPrint()));
        }

        public void Execute(string input, bool interactive = false)
        {
            if (input.StartsWith(TypeOfCommand))
            {
                var name = input.Substring(TypeOfCommand.Length);

                Write(_env.ValueTypes.ContainsKey(name)
                    ? _env.ValueTypes[name].PrettyPrint()
                    : string.Format("{0} is not defined", name));

                return;
            }
            try
            {
                UnsafeExecute(input, interactive);   
            }
            catch (StackedException ex)
            {
                if(ex.Trace.Count > 0)
                    ErrorWrite(string.Format(Resources.Program_Execute_Error, ex.Message) + "\n\t" + string.Join("\n\t", ex.Trace));
                else
                    ErrorWrite(string.Format(Resources.Program_Execute_Error, ex.Message));
            }
            catch (Exception ex)
            {
                ErrorWrite(string.Format(Resources.Program_Execute_Error, ex.Message));
            }
        }

        public void AddString(string input)
        {
            foreach (var instr in input.Split(new[] {"\r\n\r\n", "\n\n"}, StringSplitOptions.RemoveEmptyEntries))
                Evaluate(instr);
        }

        public void AddStd()
        {
            AddString(Resources.StdLib);
            AddString(Resources.DataStructures);
        }

        public void AddImpure()
        {
            Evaluate("data Ref a case Ref of a");
            Evaluate("def get (Ref x) as x");
            Evaluate("def set (Ref x) y as if x == y then ()"); // Force the correct type
            _env.Bind("set", new FunctionValue(rf => new FunctionValue(val =>
            {
                ((DataValue) rf.WHNFValue).Arguments[0] = val;
                return Defaults.Unit;
            })));

            Evaluate("def modify f x as set x (f (get x))");
            Evaluate("def print a as ()");
            _env.Bind("print", new FunctionValue(val =>
            {
                Write(val.NormalFormValue.ToString());
                return Defaults.Unit;
            }));
            Evaluate("def putStr x as if head x == 'a' then ()"); // Force the correct type
            _env.Bind("putStr", new FunctionValue(val =>
            {
                var sb = new StringBuilder();
                
                for (var evaluated = (DataValue)val.NormalFormValue; evaluated.Constructor.Name != "Nil"; evaluated = (DataValue) evaluated.Arguments[1].NormalFormValue)
                {
                    sb.Append(((CharacterValue) evaluated.Arguments[0].NormalFormValue).Value);
                }

                Write(sb.ToString());
                return Defaults.Unit;
            }));
            Evaluate("def operator ; x y as y");
            _env.Bind(";", new FunctionValue(x => new FunctionValue(y =>
            {
                x.Value = x.WHNFValue;
                return y;
            })));
            Evaluate("def operator ;; x y as y");
            _env.Bind(";;", new FunctionValue(x => new FunctionValue(y =>
            {
                x.Value = x.NormalFormValue;
                return y;
            })));
            Evaluate(@"def putStrLn x as putStr x; putStr ""\n""");
        }
    }
}