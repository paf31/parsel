using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// Some useful extension methods for working with parsers
    /// </summary>
    public static class IParserExtensions
    {
        /// <summary>
        /// p1 then p2, returning the value on the left
        /// </summary>
        public static IParser<T> ThenL<T, T1>(this IParser<T> p1, IParser<T1> p2)
        {
            return p1.Then(p2, (t, _) => t);
        }

        /// <summary>
        /// p1 then p2, returning the value on the right
        /// </summary>
        public static IParser<T1> ThenR<T, T1>(this IParser<T> p1, IParser<T1> p2)
        {
            return p1.Then(p2, (_, t) => t);
        }

        /// <summary>
        /// l then p then r, returning the value from p
        /// </summary>
        public static IParser<T> Between<T, T1>(this IParser<T> parser, IParser<T1> l, IParser<T1> r)
        {
            return l.ThenR(parser).ThenL(r);
        }

        /// <summary>
        /// p repeated, delimited by sep
        /// </summary>
        public static IParser<T[]> SepBy<T, T1>(this IParser<T> parser, IParser<T1> sep)
        {
            return parser.Then(sep.ThenR(parser).Star(), (t, ts) => new[] { t }.Concat(ts).ToArray());
        }

        /// <summary>
        /// Repeat p until terminator matches
        /// </summary>
        public static IParser<T[]> Until<T, T1>(this IParser<T> parser, IParser<T1> terminator)
        {
            return Parsers.Not(terminator).ThenR(parser).Star().ThenL(terminator);
        }
    }
}
