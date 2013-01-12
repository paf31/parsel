﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class Star<T> : IParser<T[]>
    {
        public IParser<T> Parser { get; set; }

        internal Star() { }

        public void Perform(IParserAction a)
        {
            a.Perform(this);
        }

        public R Apply<R>(IParserFunc<R> f)
        {
            return f.Apply(this);
        }

        public Expression Compile(Expression input, Expression parsers, SuccessContinuation onSuccess, FailureContinuation onFailure)
        {
            var brk = Expression.Label();

            var input1 = Expression.Variable(typeof(IndexedString));
            var results = Expression.Variable(typeof(ICollection<T>));

            var body = Parser.Compile(input1, parsers,
                (remainingInput, output) => Expression.Block(
                    Expression.Call(results, "Add", Type.EmptyTypes, output),
                    Expression.Assign(input1, remainingInput)),
                (_1, _2) => Expression.Break(brk));

            return Expression.Block(
                new ParameterExpression[] { input1, results },
                new Expression[] 
                    {
                        Expression.Assign(input1, input),
                        Expression.Assign(results, Expression.New(typeof(List<T>))),
                        Expression.Loop(body, brk), 
                        onSuccess(input1, Expression.Call(typeof(Enumerable), "ToArray", new[] { typeof(T) }, results))
                    });
        }
    }
}
