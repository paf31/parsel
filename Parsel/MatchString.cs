using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class MatchString : ParserBase<string>
    {
        public string String { get; set; }

        internal MatchString() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            var head = Expression.Call(input, "Substring", Type.EmptyTypes, new[] { Expression.Constant(0), Expression.Constant(String.Length) });
            var tail = Expression.Call(input, "Shift", Type.EmptyTypes, Expression.Constant(String.Length));

            return Expression.IfThenElse(
                Expression.LessThan(Expression.Property(input, "Length"), Expression.Constant(String.Length)),
                onFailure(input, Expression.Constant(string.Format("Expected '{0}', met EOF", String))),
                Expression.IfThenElse(
                    Expression.Equal(head, Expression.Constant(String)),
                    onSuccess(tail, head),
                    onFailure(input, Expression.Call(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) }),
                        Expression.Constant(string.Format("Expected '{0}', found '{{0}}'", String)), head))));
        }
    }
}
