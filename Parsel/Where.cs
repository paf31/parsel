using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Where<T> : IParser<T>
    {
        public IParser<T> Parser { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }

        public Expression<Func<T, string>> ErrorMessage { get; set; }

        internal Where() { }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }

        public Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return Parser.Compile(input, parsers,
                (remainingInput, output) =>
                    Expression.IfThenElse(Predicate.Apply(output),
                        onSuccess(remainingInput, output),
                        onFailure(input, Expression.Constant("Assertion failed"))),
                onFailure);
        }
    }
}
