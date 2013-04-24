using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which invokes another parser repeatedly until it fails
    /// </summary>
    public class Star<T> : ParserBase<T[]>
    {
        public IParser<T> Parser { get; set; }

        internal Star() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            var brk = Expression.Label();

            var input1 = Expression.Variable(typeof(IndexedString));
            var results = Expression.Variable(typeof(ICollection<T>));

            var body = Parser.Compile(input1, parsers,
                (remainingInput, output) => Expression.Block(
                    Expression.Call(results, "Add", Type.EmptyTypes, output),
                    Expression.Assign(input1, remainingInput)),
                (_1, _2) => Expression.Break(brk),
                productions);

            return Expression.Block(
                new ParameterExpression[] { input1, results },
                new Expression[] 
                    {
                        Expression.Assign(input1, input),
                        Expression.Assign(results, Expression.New(typeof(List<T>))),
                        Expression.Loop(body, brk), 
                        onSuccess(input1, Expression.Call(typeof(Enumerable), "ToArray", new[] { typeof(T) }, results))
                    });
        }
    }
}
