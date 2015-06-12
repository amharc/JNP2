using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MobLangTests
{
    [TestClass]
    public class ExceptionsTest
    {
        private readonly MobLang.Program _program;

        public ExceptionsTest()
        {
            _program = new MobLang.Program();
            _program.AddStd();
            _program.AddImpure();
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.OccursCheckException))]
        public void OccursCheck()
        {
            _program.UnsafeExecute("def f x as x x");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.UnboundNameException))]
        public void UnboundName()
        {
            _program.UnsafeExecute("def f x as if x == 0 then jdskhdksfhjkhjfksdkjfkjshjdkhfdkjhjfkskfdshkjdfhs");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void UnifyIntCondition()
        {
            _program.UnsafeExecute("if 42 then 0");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void UnifyIfBranches()
        {
            _program.UnsafeExecute("def f x as if x then [(42, True)] else [(True, 42)]");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void UnifyEqualityListInt()
        {
            _program.UnsafeExecute("[] == 42");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void UnifyEqualityListTuple()
        {
            _program.UnsafeExecute("(2, 3) == (2, 3, True)");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void UnifyDataConstructors()
        {
            _program.UnsafeExecute("Nothing == []");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void UnifyPatterns()
        {
            _program.UnsafeExecute("def f x as match x with 42 then () with Nil then ()");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.FunctionValueComparisonException))]
        public void CompareFunctions()
        {
            _program.UnsafeExecute("id == id");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.PatternArgumentCountException))]
        public void PatternArgumentCount()
        {
            _program.UnsafeExecute("match [] with Nil then 42 with Cons then 8");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.ParserException))]
        public void IllegalCharacter()
        {
            _program.UnsafeExecute("`");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.InvalidCharStringLiteralException))]
        public void UnterminatedStringLiteral()
        {
            _program.UnsafeExecute(@"""hello");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.InvalidCharStringLiteralException))]
        public void UnterminatedCharLiteral()
        {
            _program.UnsafeExecute("'a");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.InvalidCharStringLiteralException))]
        public void EmptyCharLiteral()
        {
            _program.UnsafeExecute("''");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.InvalidCharStringLiteralException))]
        public void TooLongCharacterLiteral()
        {
            _program.UnsafeExecute("'hello world'");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void HigherKindedDataType()
        {
            _program.UnsafeExecute("data Error case Error of List");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.BottomException))]
        public void BottomNormalForm()
        {
            _program.UnsafeExecute("def undefined as undefined in undefined");
        }

        [TestMethod]
        [ExpectedException(typeof(MobLang.Exceptions.BottomException))]
        public void BottomWeakHeadNormalForm()
        {
            _program.UnsafeExecute("def undefined as undefined in (undefined; 42)");
        }


        [TestMethod]
        [ExpectedException(typeof(MobLang.Exceptions.BottomException))]
        public void BottomCompareRhs()
        {
            _program.UnsafeExecute("def undefined as undefined in 42 >= undefined");
        }

        [TestMethod]
        [ExpectedException(typeof(MobLang.Exceptions.BottomException))]
        public void BottomCompareLhs()
        {
            _program.UnsafeExecute("def undefined as undefined in undefined < 'ą'");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.RigidTypeException))]
        public void RigidArgumentUnify()
        {
            _program.UnsafeExecute("def foo (x : a) as x + 5 > 2");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.RigidTypeException))]
        public void RigidFunctionUnify()
        {
            _program.UnsafeExecute("def foo x : a -> Bool as x == True");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.RigidTypeException))]
        public void RigidFunctionUnifyTwo()
        {
            _program.UnsafeExecute("def foo x y : a -> b -> Bool as x == y");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.UnsuccessfulMatchException))]
        public void UnsuccessfulMatch()
        {
            _program.UnsafeExecute(@"match ""hello"" with Nil then () with Cons _ Nil then ()");
        }

        [TestMethod]
        [ExpectedException(typeof(MobLang.Exceptions.UnsuccessfulMatchException))]
        public void UnsuccessfulMatchHead()
        {
            _program.UnsafeExecute("head []");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.DataTypeException))]
        public void ShadowType()
        {
            _program.UnsafeExecute("data List");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.DataTypeException))]
        public void UnboundTypeInData()
        {
            _program.UnsafeExecute("data L case L of a");
        }

        [TestMethod]
        [ExpectedException(typeof (MobLang.Exceptions.TypeException))]
        public void DeeplyNestedTypeError()
        {
            _program.UnsafeExecute(@"
def foo x y as
    match (x, y)
        with (Nil, n) if powmod 2 n 31 == 0 then n
        with (_, n) then if (fun _ as n == []) () then 42 else (match 42 with 2 if True then 0)");
        }
    }
}
