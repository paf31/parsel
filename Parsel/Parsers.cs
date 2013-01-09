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

        public static IParser<T> Where<T>(this IParser<T> p, Expression<Func<T, bool>> predicate, Expression<Func<T, string>> errorMessage)
        {
            return new Where<T> { Parser = p, Predicate = predicate, ErrorMessage = errorMessage };
        }
    }

    public interface IParser
    {
        void Perform(IParserAction a);

        R Apply<R>(IParserFunc<R> f);
    }

    public interface IParserAction
    {
        void Perform<T>(IParser<T> p);
    }

    public interface IParserFunc<R>
    {
        R Apply<T>(IParser<T> p);
    }

    public interface IParser<T> : IParser
    {
        void Accept(IParserVisitor visitor);

        R Accept<R>(IParserVisitor<R> visitor);
    }

    public interface IParserVisitor
    {
        void Visit<T>(Return<T> parser);

        void Visit<S, T>(Select<S, T> parser);

        void Visit<S, T, V>(Then<S, T, V> parser);

        void Visit<T>(Where<T> parser);

        void Visit(AnyChar parser);

        void Visit(MatchChar parser);

        void Visit(MatchString parser);

        void Visit<T>(Star<T> parser);

        void Visit<T>(Or<T> parser);

        void Visit<T>(Named<T> parser);
    }

    public interface IParserVisitor<R>
    {
        R Visit<T>(Return<T> parser);

        R Visit<S, T>(Select<S, T> parser);

        R Visit<S, T, V>(Then<S, T, V> parser);

        R Visit<T>(Where<T> parser);

        R Visit(AnyChar parser);

        R Visit(MatchChar parser);

        R Visit(MatchString parser);

        R Visit<T>(Star<T> parser);

        R Visit<T>(Or<T> parser);

        R Visit<T>(Named<T> parser);
    }

    public class Return<T> : IParser<T>
    {
        public T ReturnValue { get; set; }

        internal Return() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class Select<S, T> : IParser<T>
    {
        public IParser<S> Parser { get; set; }

        public Expression<Func<S, T>> Selector { get; set; }

        internal Select() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class Then<S, T, V> : IParser<V>
    {
        public IParser<S> First { get; set; }

        public IParser<T> Second { get; set; }

        public Expression<Func<S, T, V>> Selector { get; set; }

        internal Then() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class Where<T> : IParser<T>
    {
        public IParser<T> Parser { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }

        public Expression<Func<T, string>> ErrorMessage { get; set; }

        internal Where() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class AnyChar : IParser<char>
    {
        internal AnyChar() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class MatchChar : IParser<char>
    {
        public char Char { get; set; }

        internal MatchChar() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class MatchString : IParser<string>
    {
        public string String { get; set; }

        internal MatchString() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class Star<T> : IParser<T[]>
    {
        public IParser<T> Parser { get; set; }

        internal Star() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class Or<T> : IParser<T>
    {
        public IParser<T> Left { get; set; }

        public IParser<T> Right { get; set; }

        internal Or() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }

    public class Named<T> : IParser<T>
    {
        public string Name { get; set; }

        internal Named() { }

        public void Accept(IParserVisitor visitor)
        {
            visitor.Visit(this);
        }

        public R Accept<R>(IParserVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }
}
