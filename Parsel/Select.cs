using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Select<S, T> : IParser<T>
    {
        public IParser<S> Parser { get; set; }

        public Expression<Func<S, T>> Selector { get; set; }

        internal Select() { }

        public Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return Parser.Compile(input, parsers,
                (remainingInput, output) => onSuccess(remainingInput, Selector.Apply(output)),
                onFailure);
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
