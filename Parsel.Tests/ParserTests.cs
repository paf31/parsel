using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parsel.Tests
{
    [TestClass]
    public class ParserTests
    {
        public static void AssertMatch<T>(CompiledParser<T> p, string name, string s, string expectedRemainingInput = null)
        {
            var result = p(new IndexedString { String = s, StartAt = 0 }, null);

            Assert.IsTrue(result.Success,
                string.Format("{0} should match \"{1}\"", name, s));
            if (expectedRemainingInput != null)
            {
                Assert.AreEqual(expectedRemainingInput, result.RemainingInput.String,
                    string.Format("Expected remaining input \"{0}\", found \"{1}\".", expectedRemainingInput, result.RemainingInput.String));
            }
        }

        public static void AssertNoMatch<T>(CompiledParser<T> p, string name, string s)
        {
            Assert.IsFalse(p(new IndexedString { String = s, StartAt = 0 }, null).Success,
                string.Format("{0} should not match \"{1}\"", name, s));
        }

        [TestMethod]
        public void TestMatchChar()
        {
            var parsers = new Dictionary<string, IParser> { { "a", Parsers.MatchChar('a') } };

            var compiled = parsers.Compile()["a"] as CompiledParser<char>;

            AssertMatch(compiled, "a", "a");
            AssertMatch(compiled, "a", "abc");
            AssertNoMatch(compiled, "a", "b");
            AssertNoMatch(compiled, "a", "bcd");
            AssertNoMatch(compiled, "a", "");
        }

        [TestMethod]
        public void TestMatchString()
        {
            var parsers = new Dictionary<string, IParser> { { "Test", Parsers.MatchString("Test") } };

            var compiled = parsers.Compile()["Test"] as CompiledParser<string>;

            AssertMatch(compiled, "Test", "Test");
            AssertMatch(compiled, "Test", "Testing");
            AssertNoMatch(compiled, "Test", "Foo");
            AssertNoMatch(compiled, "Test", "");
        }

        [TestMethod]
        public void TestOr()
        {
            var parsers = new Dictionary<string, IParser> { { "fooOrBar", Parsers.MatchString("Foo").Or(Parsers.MatchString("Bar")) } };

            var compiled = parsers.Compile()["fooOrBar"] as CompiledParser<string>;

            AssertMatch(compiled, "fooOrBar", "Foo");
            AssertMatch(compiled, "fooOrBar", "Bar");
            AssertNoMatch(compiled, "fooOrBar", "Baz");
            AssertNoMatch(compiled, "fooOrBar", "");
        }

        [TestMethod]
        public void TestStar()
        {
            var parsers = new Dictionary<string, IParser> { { "xOrYStar", Parsers.MatchChar('x').Or(Parsers.MatchChar('y')).Star() } };

            var compiled = parsers.Compile()["xOrYStar"] as CompiledParser<char[]>;

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

            var parsers = new Dictionary<string, IParser> { { "selectWhere", selectWhere } };

            var compiled = parsers.Compile()["selectWhere"] as CompiledParser<int>;

            AssertMatch(compiled, "selectWhere", "123");
            AssertNoMatch(compiled, "selectWhere", "abc");
        }

        [TestMethod]
        public void TestWhere()
        {
            var parsers = new Dictionary<string, IParser> { { "xWhereXIsX", from x in Parsers.AnyChar() where x == 'x' select x } };

            var compiled = parsers.Compile()["xWhereXIsX"] as CompiledParser<char>;

            AssertMatch(compiled, "xWhereXIsX", "x");
            AssertNoMatch(compiled, "xWhereXIsX", "y");
            AssertNoMatch(compiled, "xWhereXIsX", "");
        }

        [TestMethod]
        public void TestThen()
        {
            var then = Parsers.MatchString("Foo")
                .Then(Parsers.MatchString("Bar"), (t1, t2) => t1 + t2);

            var parsers = new Dictionary<string, IParser> { { "then", then } };

            var compiled = parsers.Compile()["then"] as CompiledParser<string>;

            AssertMatch(compiled, "then", "FooBar");
        }

        [TestMethod]
        public void TestStarThenChar()
        {
            var then = Parsers.MatchChar('a').Star()
                .Then(Parsers.MatchChar('b'), (t1, t2) => 1);

            var parsers = new Dictionary<string, IParser> { { "starThenChar", then } };

            var compiled = parsers.Compile()["starThenChar"] as CompiledParser<int>;

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
            var digits = from cs in (from c in Parsers.AnyChar() where char.IsDigit(c) select c).Star()
                         where cs.Length > 0
                         select int.Parse(new string(cs));

            var whitespace = Parsers.MatchChar(' ').Star();

            var thens = digits
                .Then(whitespace, (d, _) => d)
                .Then(Parsers.MatchChar('+'), (d, _) => d)
                .Then(whitespace, (d, _) => d)
                .Then(digits, (d1, d2) => d1 + d2);

            var parsers = new Dictionary<string, IParser> { { "thens", thens } };

            var compiled = parsers.Compile()["thens"] as CompiledParser<int>;

            AssertMatch(compiled, "thens", "123 + 456");
        }
    }
}
