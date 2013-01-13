using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public delegate ParseResult<T> CompiledParser<T>(IndexedString input);

    public delegate ParseResult<T> PreCompiledParser<T>(IndexedString input, Delegate[] parsers);
 
    public delegate Expression SuccessContinuation(Expression remainingInput, Expression output);

    public delegate Expression FailureContinuation(Expression remainingInput, Expression errorMessage);
}
