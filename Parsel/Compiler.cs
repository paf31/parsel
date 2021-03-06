﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Parsel
{
    /// <summary>
    /// Extension methods which can be used to compile the data representation of a parser into
    /// a delegate
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// Compiles a parser
        /// </summary>
        public static CompiledParser<T> Compile<T>(this IParser<T> parser)
        {
            var preCompiled = (PreCompiledParser<T>)parser.Apply(new CompileAction(new string[0]));

            return input => preCompiled(input, new Delegate[0]);
        }

        /// <summary>
        /// Compile a collection of mutually dependent parsers into a collection of delegates.
        /// Dynamic methods cannot call other dynamic methods, so we need to name other parsers and pass them around in 
        /// a Dictionary<,> in order to create recursive methods.
        /// </summary>
        public static IDictionary<string, Delegate> Compile(this IDictionary<string, IParser> productions)
        {
            var compiledProductions = new Dictionary<string, Delegate>();

            foreach (var production in productions)
            {
                compiledProductions[production.Key] = production.Value.Apply(new CompileAction(productions.Keys.ToArray()));
            }

            var partiallyAppliedCompiledProductions = new Dictionary<string, Delegate>();

            foreach (var production in productions)
            {
                partiallyAppliedCompiledProductions[production.Key] = production.Value.Apply(
                    new PartiallyApplyParsersFunc(compiledProductions[production.Key], compiledProductions.Values.ToArray()));
            }

            return partiallyAppliedCompiledProductions;
        }

        /// <summary>
        /// Compile all parsers declared in a class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDictionary<string, Delegate> Compile(Type type)
        {
            var productions = new Dictionary<string, Parsel.IParser>();

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.IsStatic &&
                    !method.GetParameters().Any() &&
                    !method.GetGenericArguments().Any() &&
                    typeof(IParser).IsAssignableFrom(method.ReturnType.GetGenericTypeDefinition()))
                {
                    productions[method.Name] = (IParser)method.Invoke(null, new object[0]);
                }
            }

            return productions.Compile();
        }

        /// <summary>
        /// A polymorphic function which compiles a parser to a delegate with the correct return type
        /// </summary>
        private class CompileAction : IParserFunc<Delegate>
        {
            private readonly string[] productionNames;

            public CompileAction(string[] productionNames)
            {
                this.productionNames = productionNames;
            }

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
                var parsers = Expression.Parameter(typeof(Delegate[]), "parsers");

                var body = Expression.Block(new Expression[] 
                {
                    p.Compile(input, parsers, successContinuation, failureContinuation, productionNames),
                    Expression.Label(@return, Expression.Default(typeof(ParseResult<T>))) 
                });

                var lambda = Expression.Lambda<PreCompiledParser<T>>(body, input, parsers);

                return lambda.Compile();
            }
        }

        /// <summary>
        /// A polymorphic function which partially applies a compiled parser
        /// </summary>
        private class PartiallyApplyParsersFunc : IParserFunc<Delegate>
        {
            private readonly Delegate compiledParser;
            private readonly Delegate[] compiledProductions;

            public PartiallyApplyParsersFunc(Delegate compiledParser, Delegate[] compiledProductions)
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
