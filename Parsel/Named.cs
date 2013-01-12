using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Named<T> : IParser<T>
    {
        public string Name { get; set; }

        internal Named() { }

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
            var compiledParser = Expression.TypeAs(
                    Expression.MakeIndex(parsers,
                        typeof(IDictionary<string, Delegate>).GetProperty("Item"),
                        new[] { Expression.Constant(Name) }),
                    typeof(PreCompiledParser<T>));

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
