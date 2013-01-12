using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public static class Parsers
    {
        public static IParser<T> Return<T>(T returnValue)
        {
            return new Return<T> { ReturnValue = returnValue };
        }

        public static IParser<char> AnyChar()
        {
            return new AnyChar();
        }

        public static IParser<char> MatchChar(char c)
        {
            return new MatchChar { Char = c };
        }

        public static IParser<string> MatchString(string s)
        {
            return new MatchString { String = s };
        }

        public static IParser<T[]> Star<T>(this IParser<T> p)
        {
            return new Star<T> { Parser = p };
        }

        public static IParser<T> Or<T>(this IParser<T> p1, IParser<T> p2)
        {
            return new Or<T> { Left = p1, Right = p2 };
        }

        public static IParser<T> Named<T>(string name)
        {
            return new Named<T> { Name = name };
        }

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

        public static IParser<T> Select<S, T>(this IParser<S> p, Expression<Func<S, T>> f)
        {
            return new Select<S, T> { Parser = p, Selector = f };
        }

        public static IParser<V> Then<S, T, V>(this IParser<S> first, IParser<T> second, Expression<Func<S, T, V>> selector)
        {
            return new Then<S, T, V> { First = first, Second = second, Selector = selector };
        }

        public static IParser<T> Where<T>(this IParser<T> p, Expression<Func<T, bool>> predicate)
        {
            return p.Where(predicate, _ => "Assertion failed.");
        }

        public static IParser<Unit> Not<T>(this IParser<T> p)
        {
            return new Not<T> { Parser = p };
        }

        public static IParser<T> Where<T>(this IParser<T> p, Expression<Func<T, bool>> predicate, Expression<Func<T, string>> errorMessage)
        {
            return new Where<T> { Parser = p, Predicate = predicate, ErrorMessage = errorMessage };
        }
    }
}
