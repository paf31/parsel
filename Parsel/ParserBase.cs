using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public abstract class ParserBase<T> : IParser<T>
    {
        public abstract Expression Compile(Expression input, System.Linq.Expressions.Expression parsers,
            SuccessContinuation onSuccess, FailureContinuation onFailure);

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
