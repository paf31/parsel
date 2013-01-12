using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Then<S, T, V> : ParserBase<V>
    {
        public IParser<S> First { get; set; }

        public IParser<T> Second { get; set; }

        public Expression<Func<S, T, V>> Selector { get; set; }

        internal Then() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return First.Compile(input, parsers,
                    (remainingInput1, s) => Second.Compile(remainingInput1, parsers,
                        (remainingInput2, t) => onSuccess(remainingInput2, Selector.Apply(s, t)),
                        onFailure),
                    onFailure);
        }
    }
}
