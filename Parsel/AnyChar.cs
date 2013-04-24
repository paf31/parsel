using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which matches any character successfully
    /// </summary>
    public class AnyChar : ParserBase<char>
    {
        internal AnyChar() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            var head = Expression.MakeIndex(input, typeof(IndexedString).GetProperty("Item"), new[] { Expression.Constant(0) });
            var tail = Expression.Call(input, "Shift", Type.EmptyTypes, Expression.Constant(1));

            var test = Expression.IsFalse(Expression.Property(input, "IsEmpty"));
            var then = onSuccess(tail, head);
            var @else = onFailure(input, Expression.Constant("Unexpected EOF"));

            return Expression.IfThenElse(test, then, @else);
        }
    }
}
