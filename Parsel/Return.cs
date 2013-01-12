using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Return<T> : IParser<T>
    {
        public T ReturnValue { get; set; }

        internal Return() { }

        public Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return onSuccess(input, Expression.Constant(ReturnValue, typeof(T)));
        }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }
    }
}
