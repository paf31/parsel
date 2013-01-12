﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Select<S, T> : ParserBase<T>
    {
        public IParser<S> Parser { get; set; }

        public Expression<Func<S, T>> Selector { get; set; }

        internal Select() { }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return Parser.Compile(input, parsers,
                (remainingInput, output) => onSuccess(remainingInput, Selector.Apply(output)),
                onFailure);
        }
    }
}
