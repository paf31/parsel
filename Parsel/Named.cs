using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which invokes another parser by name
    /// </summary>
    public class Named<T> : ParserBase<T>
    {
        public string Name { get; set; }

        internal Named() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            int index = Array.IndexOf(productions, Name);

            var compiledParser = Expression.TypeAs(Expression.ArrayIndex(parsers, Expression.Constant(index)), typeof(PreCompiledParser<T>));

            var result = Expression.Variable(typeof(ParseResult<T>), "result");

            return Expression.Block(new[] { result },
                new Expression[] 
                    {
                        Expression.Assign(result, Expression.Invoke(compiledParser, input, parsers)),
                        Expression.IfThenElse(
                            Expression.Property(result, "Success"),
                            onSuccess(Expression.Property(result, "RemainingInput"), Expression.Property(result, "Output")),
                            onFailure(input, Expression.Property(result, "ErrorMessage")))
                    });
        }
    }
}
