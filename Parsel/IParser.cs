using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public interface IParser
    {
        void Perform(IParserAction a);

        R Apply<R>(IParserFunc<R> f);
    }

    public interface IParserAction
    {
        void Perform<T>(IParser<T> p);
    }

    public interface IParserFunc<R>
    {
        R Apply<T>(IParser<T> p);
    }

    public interface IParser<T> : IParser
    {
        Expression Compile(Expression input,
                           Expression parsers,
                           SuccessContinuation onSuccess,
                           FailureContinuation onFailure);
    }
}
