using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// The result of a parse attempt: success with an output, or failure with an error message
    /// </summary>
    public class ParseResult<T>
    {
        private readonly bool success;
        private readonly IndexedString remainingInput;
        private readonly T output;
        private readonly string errorMessage;

        internal ParseResult(bool success, IndexedString remainingInput, T output, string errorMessage)
        {
            this.success = success;
            this.remainingInput = remainingInput;
            this.output = output;
            this.errorMessage = errorMessage;
        }

        public bool Success
        {
            get { return success; }
        }

        public IndexedString RemainingInput
        {
            get { return remainingInput; }
        }

        public T Output
        {
            get { return output; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
        }
    }

    /// <summary>
    /// Helper methods for constructing ParseResults
    /// </summary>
    public static class ParseResult
    {
        public static ParseResult<T> Success<T>(IndexedString remainingInput, T output)
        {
            return new ParseResult<T>(true, remainingInput, output, null);
        }

        public static ParseResult<T> Failure<T>(IndexedString remainingInput, string errorMessage)
        {
            return new ParseResult<T>(false, remainingInput, default(T), errorMessage);
        }
    }
}
