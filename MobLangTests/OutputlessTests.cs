using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MobLangTests
{
    [TestClass]
    public class OutputlessTests
    {
        private readonly MobLang.Program _program;
        public OutputlessTests()
        {
            _program = new MobLang.Program();
            _program.AddStd();
            _program.AddImpure();
            _program.UnsafeExecute("def undefined : undefined as undefined");
            _program.UnsafeExecute("def assert x as if not x then undefined");
            _program.UnsafeExecute("def assert_eq x y as assert (x == y)");
        }

        [TestMethod]
        public void DataType()
        {
            _program.UnsafeExecute("data X a case X of Int -> Int case Y of (Int, Bool -> a)");
            _program.UnsafeExecute("X id");
            _program.UnsafeExecute("Y (42 + 12, id)");
        }

        [TestMethod]
        public void DefWithRigidType()
        {
            _program.UnsafeExecute("def f x : a -> Int as if f 5 == f 6 then 42 else -42");
            _program.UnsafeExecute("def y as f ()");
            _program.UnsafeExecute("def z as f (Just 42)");
        }

        [TestMethod]
        public void Builtins()
        {
            _program.UnsafeExecute(@"Just ['h']");
            _program.UnsafeExecute("map (fun y as y + 3) $ map (fun x as x 7) $ map (operator+) [1, 2, 3]");
            _program.UnsafeExecute("def fibs as 0 :: 1 :: zipWith (operator+) fibs (tail fibs)");
            _program.UnsafeExecute("assert_eq [0, 1, 1, 2, 3, 5, 8, 13] $ take 8 fibs");
            _program.UnsafeExecute("assert_eq 100 $ length $ take 100 fibs");
            _program.UnsafeExecute("assert_eq 8 $ filter (fun x as x % 2 == 0) fibs !! 2");
            _program.UnsafeExecute("assert_eq 8 $ powmod 4242424242 42424242 31");
        }

        [TestMethod]
        public void Laziness()
        {
            _program.UnsafeExecute("assert $ Just undefined < Nothing");
            _program.UnsafeExecute("assert $ (3, Just undefined, undefined) < (3, Nothing, undefined : List Float)");
            _program.UnsafeExecute("match 7 with n if n % 2 == 0 then undefined with n if n % 3 == 0 then undefined with _ then 42");
        }

        [TestMethod]
        public void Impure()
        {
            _program.UnsafeExecute("def ref as Ref 42");
            _program.UnsafeExecute("assert_eq 42 $ get ref");
            _program.UnsafeExecute("set ref (-42)");
            _program.UnsafeExecute("assert_eq (-42) $ get ref");
            _program.UnsafeExecute("def lst as replicate 3 ref");
            _program.UnsafeExecute("assert_eq [-42, -42, -42] $ map get lst");
            _program.UnsafeExecute("set ref 0");
            _program.UnsafeExecute("assert_eq [0, 0, 0] $ map get lst");
        }

        [TestMethod]
        public void Annotated()
        {
            _program.UnsafeExecute("def f x as match x with n then n == (x : Int)");
            _program.UnsafeExecute("assert_eq True $ f 42");
        }

    }
}
