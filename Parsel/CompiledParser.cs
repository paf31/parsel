using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    public delegate ParseResult<T> CompiledParser<T>(IndexedString input, IDictionary<string, Delegate> parsers);
}
