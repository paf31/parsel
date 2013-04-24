using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which succeeds if and only if another parser fails
    /// </summary>
    public class Not<T> : ParserBase<Unit>
    {
        public IParser<T> Parser { get; set; }

        internal Not() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            return Parser.Compile(input, parsers, 
                (remainingInput, output) => onFailure(input, Expression.Constant("Assertion failed.")),
                (remainingInput, errorMessage) => onSuccess(input, Expression.Constant(Unit.Value)),
                productions); 
        }
    }
}
