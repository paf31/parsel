using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parsel
{
    public static class IParserExtensions
    {
        public static IParser<T> ThenL<T, T1>(this IParser<T> p1, IParser<T1> p2)
        {
            return p1.Then(p2, (t, _) => t);
        }

        public static IParser<T1> ThenR<T, T1>(this IParser<T> p1, IParser<T1> p2)
        {
            return p1.Then(p2, (_, t) => t);
        }

        public static IParser<T> Between<T, T1>(this IParser<T> parser, IParser<T1> l, IParser<T1> r)
        {
            return l.ThenR(parser).ThenL(r);
        }

        public static IParser<T[]> SepBy<T, T1>(this IParser<T> parser, IParser<T1> sep)
        {
            return parser.Then(sep.ThenR(parser).Star(), (t, ts) => new[] { t }.Concat(ts).ToArray());
        }

        public static IParser<T[]> Until<T, T1>(this IParser<T> parser, IParser<T1> terminator)
        {
            return Parsers.Not(terminator).ThenR(parser).Star().ThenL(terminator);
        }
    }
}
