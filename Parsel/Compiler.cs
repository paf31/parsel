using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Parsel
{
    public static class Compiler
    {
        public static IDictionary<string, Delegate> Compile(this IDictionary<string, IParser> productions)
        {
            var compiledProductions = new Dictionary<string, Delegate>();

            foreach (var production in productions)
            {
                compiledProductions[production.Key] = production.Value.Apply(new CompileAction());
            }

            var partiallyAppliedCompiledProductions = new Dictionary<string, Delegate>();

            foreach (var production in productions)
            {
                partiallyAppliedCompiledProductions[production.Key] = production.Value.Apply(new PartiallyApplyParsersFunc(compiledProductions[production.Key], compiledProductions));
            }

            return partiallyAppliedCompiledProductions;
        }

        private class CompileAction : IParserFunc<Delegate>
        {
            public Delegate Apply<T>(IParser<T> p)
            {
                LabelTarget @return = Expression.Label(typeof(ParseResult<T>));

                SuccessContinuation successContinuation = (remainingInput, output) =>
                    Expression.Return(@return,
                        Expression.Call(typeof(ParseResult).GetMethod("Success").MakeGenericMethod(typeof(T)), remainingInput, output));

                FailureContinuation failureContinuation = (remainingInput, errorMessage) =>
                     Expression.Return(@return,
                        Expression.Call(typeof(ParseResult).GetMethod("Failure").MakeGenericMethod(typeof(T)), remainingInput, errorMessage));
                
                var input = Expression.Parameter(typeof(IndexedString), "input");
                var parsers = Expression.Parameter(typeof(IDictionary<string, Delegate>), "parsers");

                var body = Expression.Block(new Expression[] 
                {
                    p.Compile(input, parsers, successContinuation, failureContinuation),
                    Expression.Label(@return, Expression.Default(typeof(ParseResult<T>))) 
                });

                var lambda = Expression.Lambda<PreCompiledParser<T>>(body, input, parsers);

                return lambda.Compile();
            }
        }

        private class PartiallyApplyParsersFunc : IParserFunc<Delegate>
        {
            private readonly Delegate compiledParser;
            private readonly IDictionary<string, Delegate> compiledProductions;

            public PartiallyApplyParsersFunc(Delegate compiledParser, Dictionary<string, Delegate> compiledProductions)
            {
                this.compiledParser = compiledParser;
                this.compiledProductions = compiledProductions;
            }

            public Delegate Apply<T>(IParser<T> p)
            {
                var compiledParserOfT = compiledParser as PreCompiledParser<T>;
                return new CompiledParser<T>(input => compiledParserOfT(input, compiledProductions));
            }
        }
    }
}
