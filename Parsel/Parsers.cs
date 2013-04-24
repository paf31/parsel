using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// Static factory methods used for building up parsers
    /// </summary>
    public static class Parsers
    {
        /// <summary>
        /// A parser which always succeeds
        /// </summary>
        public static IParser<T> Return<T>(T returnValue)
        {
            return new Return<T> { ReturnValue = returnValue };
        }

        /// <summary>
        /// A parser which always fails
        /// </summary>
        public static IParser<T> Fail<T>(string errorMessage)
        {
            return new Fail<T> { ErrorMessage = errorMessage };
        }
        
        /// <summary>
        /// A parser which matches any characters successfully
        /// </summary>
        public static IParser<char> AnyChar()
        {
            return new AnyChar();
        }

        /// <summary>
        /// A parser which matches a specific char
        /// </summary>
        public static IParser<char> MatchChar(char c)
        {
            return new MatchChar { Char = c };
        }

        /// <summary>
        /// A parser which matches a specific string
        /// </summary>
        public static IParser<string> MatchString(string s)
        {
            return new MatchString { String = s };
        }

        /// <summary>
        /// A parser which invokes another parser until it fails
        /// </summary>
        public static IParser<T[]> Star<T>(this IParser<T> p)
        {
            return new Star<T> { Parser = p };
        }

        /// <summary>
        /// A parser which tried two alternatives
        /// </summary>
        public static IParser<T> Or<T>(this IParser<T> p1, IParser<T> p2)
        {
            return new Or<T> { Left = p1, Right = p2 };
        }

        /// <summary>
        /// A parser which invokes another parser by name
        /// </summary>
        public static IParser<T> Named<T>(string name)
        {
            return new Named<T> { Name = name };
        }

        /// <summary>
        /// A parser which invokes another parser by name - typed helper method
        /// </summary>
        public static IParser<T> Named<T>(Expression<Func<IParser<T>>> e) 
        {
            var visitor = new FindNameVisitor();
            visitor.Visit(e);
            return Parsers.Named<T>(visitor.Name);
        }

        private class FindNameVisitor : ExpressionVisitor 
        {
            public string Name { get; set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                Name = node.Method.Name;
                return base.VisitMethodCall(node);
            }
        }

        /// <summary>
        /// A parser which changes the return type by applying a function inside the result
        /// </summary>
        public static IParser<T> Select<S, T>(this IParser<S> p, Expression<Func<S, T>> f)
        {
            return new Select<S, T> { Parser = p, Selector = f };
        }

        /// <summary>
        /// A parser which combines the results of two other parsers into a single result
        /// </summary>
        public static IParser<V> Then<S, T, V>(this IParser<S> first, IParser<T> second, Expression<Func<S, T, V>> selector)
        {
            return new Then<S, T, V> { First = first, Second = second, Selector = selector };
        }

        /// <summary>
        /// A parser which applies a filter to the result
        /// </summary>
        public static IParser<T> Where<T>(this IParser<T> p, Expression<Func<T, bool>> predicate)
        {
            return p.Where(predicate, _ => "Assertion failed.");
        }

        /// <summary>
        /// A parser which succeeds iff another parser fails
        /// </summary>
        public static IParser<Unit> Not<T>(this IParser<T> p)
        {
            return new Not<T> { Parser = p };
        }

        /// <summary>
        /// A parser which applies a filter to the result and fails with a custom error message
        /// </summary>
        public static IParser<T> Where<T>(this IParser<T> p, Expression<Func<T, bool>> predicate, Expression<Func<T, string>> errorMessage)
        {
            return new Where<T> { Parser = p, Predicate = predicate, ErrorMessage = errorMessage };
        }
    }
}
