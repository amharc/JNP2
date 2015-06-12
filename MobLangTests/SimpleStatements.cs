using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MobLangTests
{
    [TestClass]
    public class SimpleStatements
    {
        private readonly MobLang.Program _program;
        private readonly StringBuilder _sb;

        public SimpleStatements()
        {
            _sb = new StringBuilder();
            _program = new MobLang.Program();
            _program.AddStd();
            _program.AddImpure();
            _program.Writer += str => _sb.Append(str);
        }

        [TestMethod]
        public void TwoPlusTwo()
        {
            _sb.Clear();
            _program.Execute("2 + 2", true);
            Assert.AreEqual("result : Int = 4", _sb.ToString());
        }

        [TestMethod]
        public void OperatorPrecedence()
        {
            _sb.Clear();
            _program.Execute("2 + 3 * 4", true);
            Assert.AreEqual("result : Int = 14", _sb.ToString());
        }


        [TestMethod]
        public void Print()
        {
            _sb.Clear();
            _program.UnsafeExecute("print (Just [42, -42], Nothing, [])");
            Assert.AreEqual("(Just ([42, -42]), Nothing, [])", _sb.ToString());
        }

        [TestMethod]
        public void PutStr()
        {
            _sb.Clear();
            _program.UnsafeExecute(@"putStr ""hello""");
            Assert.AreEqual("hello", _sb.ToString());
        }
    }
}
