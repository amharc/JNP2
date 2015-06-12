using System;
using System.Text;

namespace MobLangREPL
{
    internal class Program
    {

        private static void Main()
        {
            var instance = new MobLang.Program();
            instance.Writer += Console.WriteLine;
            instance.AddStd();
            instance.AddImpure();

            while (true)
            {
                Console.Write(@"> ");

                var builder = new StringBuilder();
                string line;

                do
                {
                    line = Console.ReadLine();
                    builder.Append(line);
                } while (line != "");

                var input = builder.ToString();
                instance.Execute(input, true);
            }
        }
    }
}