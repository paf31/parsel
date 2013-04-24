using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A parser which always succeeds
    /// </summary>
    public class Return<T> : ParserBase<T>
    {
        public T ReturnValue { get; set; }

        internal Return() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions)
        {
            return onSuccess(input, Expression.Constant(ReturnValue, typeof(T)));
        }
    }
}
