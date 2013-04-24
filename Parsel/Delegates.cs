using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// A compiled parser which takes an indexed string and returns either a successfully parsed result or an error message
    /// </summary>
    public delegate ParseResult<T> CompiledParser<T>(IndexedString input);

    /// <summary>
    /// A parser which has been partially compiled. A method of this type can be partially applied to get a CompiledParser.
    /// </summary>
    public delegate ParseResult<T> PreCompiledParser<T>(IndexedString input, Delegate[] parsers);
 
    /// <summary>
    /// The continuation used when a parser succeeds
    /// </summary>
    public delegate Expression SuccessContinuation(Expression remainingInput, Expression output);

    /// <summary>
    /// The continuation used when a parser fails
    /// </summary>
    public delegate Expression FailureContinuation(Expression remainingInput, Expression errorMessage);
}
