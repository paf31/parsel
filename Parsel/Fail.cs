﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Fail<T> : ParserBase<T>
    {
        public string ErrorMessage { get; set; }

        public override Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            return onFailure(input, Expression.Constant(ErrorMessage));
        }
    }
}