using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which applies a filter to the result of another parser
    /// </summary>
    public class Where<T> : ParserBase<T>
    {
        public IParser<T> Parser { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }

        public Expression<Func<T, string>> ErrorMessage { get; set; }

        internal Where() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            return Parser.Compile(input, parsers,
                (remainingInput, output) =>
                    Expression.IfThenElse(Predicate.Apply(output),
                        onSuccess(remainingInput, output),
                        onFailure(input, Expression.Constant("Assertion failed"))),
                onFailure, productions);
        }
    }
}
