using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A subclass of all parser implementations. 
    /// </summary>
    public abstract class ParserBase<T> : IParser<T>
    {
        public abstract Expression Compile(Expression input, Expression parsers,
            SuccessContinuation onSuccess, FailureContinuation onFailure, string[] productions);

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
