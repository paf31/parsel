using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parsel.Tests
{
    [TestClass]
    public class ParserTests
    {
        public static void AssertMatch<T>(CompiledParser<T> p, string name, string s, string expectedRemainingInput = null)
        {
            var result = p(new IndexedString(s, 0));

            Assert.IsTrue(result.Success, string.Format("{0} should match \"{1}\"", name, s));

            if (expectedRemainingInput != null)
            {
                Assert.AreEqual(expectedRemainingInput, result.RemainingInput.ToString(),
                    string.Format("Expected remaining input \"{0}\", found \"{1}\".", expectedRemainingInput, result.RemainingInput.String));
            }
        }

        public static void AssertNoMatch<T>(CompiledParser<T> p, string name, string s, IDictionary<string, Delegate> compiledProductions = null)
        {
            var result = p(new IndexedString(s, 0));

            Assert.IsFalse(result.Success, string.Format("{0} should not match \"{1}\"", name, s));
        }

        [TestMethod]
        public void TestMatchChar()
        {
            var compiled = Parsers.MatchChar('a').Compile();

            AssertMatch(compiled, "a", "a");
            AssertMatch(compiled, "a", "abc");
            AssertNoMatch(compiled, "a", "b");
            AssertNoMatch(compiled, "a", "bcd");
            AssertNoMatch(compiled, "a", "");
        }

        [TestMethod]
        public void TestMatchString()
        {
            var compiled = Parsers.MatchString("Test").Compile();

            AssertMatch(compiled, "Test", "Test");
            AssertMatch(compiled, "Test", "Testing");
            AssertNoMatch(compiled, "Test", "Foo");
            AssertNoMatch(compiled, "Test", "");
        }

        [TestMethod]
        public void TestOr()
        {
            var fooOrBar =Parsers.MatchString("Foo").Or(Parsers.MatchString("Bar"));

            var compiled = fooOrBar.Compile();

            AssertMatch(compiled, "fooOrBar", "Foo");
            AssertMatch(compiled, "fooOrBar", "Bar");
            AssertNoMatch(compiled, "fooOrBar", "Baz");
            AssertNoMatch(compiled, "fooOrBar", "");
        }

        [TestMethod]
        public void TestStar()
        {
            var xOrYStar = Parsers.MatchChar('x').Or(Parsers.MatchChar('y')).Star();

            var compiled = xOrYStar.Compile();

            AssertMatch(compiled, "xOrYStar", "xxx");
            AssertMatch(compiled, "xOrYStar", "yyy");
            AssertMatch(compiled, "xOrYStar", "xyxyx");
            AssertMatch(compiled, "xOrYStar", "");
            AssertMatch(compiled, "xOrYStar", "a");
            AssertMatch(compiled, "xOrYStar", "xa");
            AssertMatch(compiled, "xOrYStar", "ya");
            AssertMatch(compiled, "xOrYStar", "ax");
        }

        [TestMethod]
        public void TestSelectWhere()
        {
            var selectWhere = from cs in Parsers.AnyChar()
                                .Where(c => char.IsDigit(c), c => string.Format("Expected digit, found '{0}'", c)).Star()
                              where cs.Length > 0
                              select int.Parse(new string(cs));

            var compiled = selectWhere.Compile();

            AssertMatch(compiled, "selectWhere", "123");
            AssertNoMatch(compiled, "selectWhere", "abc");
        }

        [TestMethod]
        public void TestWhere()
        {
            var xWhereXIsX = from x in Parsers.AnyChar()
                             where x == 'x' 
                             select x;

            var compiled = xWhereXIsX.Compile();

            AssertMatch(compiled, "xWhereXIsX", "x");
            AssertNoMatch(compiled, "xWhereXIsX", "y");
            AssertNoMatch(compiled, "xWhereXIsX", "");
        }

        [TestMethod]
        public void TestThen()
        {
            var then = Parsers.MatchString("Foo")
                .Then(Parsers.MatchString("Bar"), (t1, t2) => t1 + t2);

            var compiled = then.Compile();

            AssertMatch(compiled, "then", "FooBar");
        }

        [TestMethod]
        public void TestStarThenChar()
        {
            var starThenChar = Parsers.MatchChar('a').Star()
                .Then(Parsers.MatchChar('b'), (t1, t2) => 1);

            var compiled = starThenChar.Compile();

            AssertMatch(compiled, "starThenChar", "b");
            AssertMatch(compiled, "starThenChar", "ab");
            AssertMatch(compiled, "starThenChar", "aab");
            AssertMatch(compiled, "starThenChar", "aaab");

            AssertNoMatch(compiled, "starThenChar", "a");
            AssertNoMatch(compiled, "starThenChar", "aa");
            AssertNoMatch(compiled, "starThenChar", "aaa");
        }

        [TestMethod]
        public void TestNestedThens()
        {
            var digits = from cs in
                             (from c in Parsers.AnyChar() where char.IsDigit(c) select c).Star()
                         where cs.Length > 0
                         select int.Parse(new string(cs));

            var whitespace = Parsers.MatchChar(' ').Star();

            var thens = digits
                .Then(whitespace, (d, _) => d)
                .Then(Parsers.MatchChar('+'), (d, _) => d)
                .Then(whitespace, (d, _) => d)
                .Then(digits, (d1, d2) => d1 + d2);

            var compiled = thens.Compile();

            AssertMatch(compiled, "thens", "123 + 456");
        }

        [TestMethod]
        public void TestNot()
        {
            var notA = Parsers.Not(Parsers.MatchChar('a'));
            
            var compiled = notA.Compile();

            AssertMatch(compiled, "notA", "");
            AssertMatch(compiled, "notA", "b");
            AssertNoMatch(compiled, "notA", "a");
        }

        private static class TestNamedParser_Parsers
        {
            public static IParser<int> A()
            {
                return Parsel.Parsers.MatchChar('a')
                    .Then(Parsel.Parsers.Named(() => B())
                        .Or(Parsers.Return(0)), (c, cs) => 1);
            }

            public static IParser<int> B()
            {
                return Parsel.Parsers.MatchChar('b')
                    .Then(Parsel.Parsers.Named(() => A())
                        .Or(Parsers.Return(0)), (c, cs) => 1);
            }
        }

        [TestMethod]
        public void TestNamedParser()
        {
            var compiledProductions = Parsel.Compiler.Compile(typeof(TestNamedParser_Parsers));

            var a = compiledProductions["A"] as Parsel.CompiledParser<int>;
            var b = compiledProductions["B"] as Parsel.CompiledParser<int>;

            AssertMatch(a, "a", "a", string.Empty);
            AssertMatch(a, "a", "ab", string.Empty);
            AssertMatch(a, "a", "aba", string.Empty);
            AssertMatch(a, "a", "abab", string.Empty);
            AssertMatch(b, "b", "b", string.Empty);
            AssertMatch(b, "b", "ba", string.Empty);
            AssertMatch(b, "b", "bab", string.Empty);
            AssertMatch(b, "b", "baba", string.Empty);
        }
    }
}
