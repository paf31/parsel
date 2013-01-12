using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Return<T> : ParserBase<T>
    {
        public T ReturnValue { get; set; }

        internal Return() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return onSuccess(input, Expression.Constant(ReturnValue, typeof(T)));
        }
    }
}
