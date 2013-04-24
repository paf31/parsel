using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// This type hides the type parameter T in IParser&lt;T&gt;
    /// </summary>
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

    /// <summary>
    /// A parser is anything which can be compiled into a delegate by taking an input parameter, 
    /// a dictionary of other parsers, and success and failure continuations. The list of other 
    /// parsers is provided in the 'productions' parameter.
    /// </summary>
    public interface IParser<out T> : IParser
    {
        Expression Compile(Expression input,
                           Expression parsers,
                           SuccessContinuation onSuccess,
                           FailureContinuation onFailure,
                           string[] productions);
    }
}
