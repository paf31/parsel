using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which tries two alternatives
    /// </summary>
    public class Or<T> : ParserBase<T>
    {
        public IParser<T> Left { get; set; }

        public IParser<T> Right { get; set; }

        internal Or() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            return Left.Compile(input, parsers,
                onSuccess,
                (_, errorMessage1) => Right.Compile(input, parsers, onSuccess,
                    (remainingInput, errorMessage2) => onFailure(input, CombineErrorMessages(errorMessage1, errorMessage2)), 
                    productions),
                productions);
        }

        private Expression CombineErrorMessages(Expression errorMessage1, Expression errorMessage2)
        {
            return Expression.Call(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) }),
                Expression.Constant("Failure at disjunction: {0}, or {1}."),
                errorMessage1, errorMessage2);
        }
    }
}
