using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which matches a specific char
    /// </summary>
    public class MatchChar : ParserBase<char>
    {
        public char Char { get; set; }

        internal MatchChar() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            var head = Expression.MakeIndex(input, typeof(IndexedString).GetProperty("Item"), new[] { Expression.Constant(0) });
            var tail = Expression.Call(input, "Shift", Type.EmptyTypes, Expression.Constant(1));

            return Expression.IfThenElse(
                Expression.IsTrue(Expression.Property(input, "IsEmpty")),
                onFailure(input, Expression.Constant(string.Format("Expected '{0}', met EOF", Char))),
                Expression.IfThenElse(
                    Expression.Equal(head, Expression.Constant(Char)),
                    onSuccess(tail, Expression.Constant(Char)),
                    onFailure(input, Expression.Call(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) }),
                        Expression.Constant(string.Format("Expected '{0}', found '{{0}}'", Char)),
                        Expression.TypeAs(head, typeof(object))))));
        }
    }
}
