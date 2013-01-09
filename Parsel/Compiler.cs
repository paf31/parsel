using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Parsel
{
    public class ParseResult<T>
    {
        public bool Success { get; set; }

        public IndexedString RemainingInput { get; set; }

        public T Output { get; set; }

        public string ErrorMessage { get; set; }

        public ParseResult(IndexedString remainingInput, T output)
        {
            Success = true;
            RemainingInput = remainingInput;
            Output = output;
        }

        public ParseResult(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }

    public delegate ParseResult<T> CompiledParser<T>(IndexedString input, IDictionary<string, Delegate> parsers);

    public delegate Expression SuccessContinuation(Expression remainingInput, Expression output);

    public delegate Expression FailureContinuation(Expression errorMessage);

    public static class Compiler
    {
        private class CompileAction : IParserFunc<Delegate>
        {
            private readonly string productionName;
            private readonly ParameterExpression input;
            private readonly ParameterExpression parsers;

            public CompileAction(string productionName, ParameterExpression input, ParameterExpression parsers)
            {
                this.productionName = productionName;
                this.input = input;
                this.parsers = parsers;
            }

            public Delegate Apply<T>(IParser<T> p)
            {
                LabelTarget @return = Expression.Label(typeof(ParseResult<T>));

                SuccessContinuation successContinuation = (remainingInput, output) =>
                    Expression.Return(@return,
                        Expression.New(
                            typeof(ParseResult<T>).GetConstructor(new[] { typeof(IndexedString), typeof(T) }),
                            remainingInput, output));

                FailureContinuation failureContinuation = errorMessage =>
                     Expression.Return(@return,
                        Expression.New(
                            typeof(ParseResult<T>).GetConstructor(new[] { typeof(string) }),
                            errorMessage));

                var visitor = new CompilerVisitor(input, parsers, successContinuation, failureContinuation);

                var body = Expression.Block(new Expression[] 
                {
                    p.Accept(visitor), 
                    Expression.Label(@return, Expression.Default(typeof(ParseResult<T>))) 
                });

                var lambda = Expression.Lambda<CompiledParser<T>>(body, input, parsers);

                return (Delegate)lambda.Compile();
            }
        }

        public static IDictionary<string, Delegate> Compile(this IDictionary<string, IParser> productions)
        {
            var input = Expression.Parameter(typeof(IndexedString), "input");
            var parsers = Expression.Parameter(typeof(IDictionary<string, Delegate>), "parsers");

            var compiledProductions = new Dictionary<string, Delegate>();

            foreach (var production in productions)
            {
                compiledProductions[production.Key] = production.Value.Apply(new CompileAction(production.Key, input, parsers));
            }

            return compiledProductions;
        }

        public class CompilerVisitor : IParserVisitor<Expression>
        {
            private readonly Expression input;
            private readonly Expression parsers;
            private readonly SuccessContinuation onSuccess;
            private readonly FailureContinuation onFailure;

            public CompilerVisitor(Expression input,
                Expression parsers,
                SuccessContinuation onSuccess,
                FailureContinuation onFailure)
            {
                this.input = input;
                this.parsers = parsers;
                this.onSuccess = onSuccess;
                this.onFailure = onFailure;
            }

            public static Expression Compile<T>(IParser<T> parser,
                Expression input,
                Expression parsers,
                SuccessContinuation onSuccess,
                FailureContinuation onFailure)
            {
                return parser.Accept(new CompilerVisitor(input, parsers, onSuccess, onFailure));
            }

            public Expression Visit<T>(Return<T> parser)
            {
                return Expression.Constant(parser.ReturnValue, typeof(T));
            }

            public Expression Visit<S, T>(Select<S, T> parser)
            {
                Func<Expression, Expression> select = s => new ReplaceParameterVisitor(parser.Selector.Parameters[0], s).Visit(parser.Selector.Body);

                return Compile(parser.Parser, input, parsers,
                    (remainingInput, output) => onSuccess(remainingInput, select(output)),
                    onFailure);
            }

            public Expression Visit<S, T, V>(Then<S, T, V> parser)
            {
                return Compile(parser.First, input, parsers,
                    (remainingInput1, s) => Compile(parser.Second, remainingInput1, parsers,
                        (remainingInput2, t) => onSuccess(remainingInput2,
                            new ReplaceParameterVisitor(parser.Selector.Parameters[0], s).Visit(
                                new ReplaceParameterVisitor(parser.Selector.Parameters[1], t).Visit(
                                    parser.Selector.Body))),
                        onFailure),
                    onFailure);
            }

            public Expression Visit<T>(Where<T> parser)
            {
                return Compile(parser.Parser, input, parsers,
                    (remainingInput, output) =>
                        Expression.IfThenElse(new ReplaceParameterVisitor(parser.Predicate.Parameters[0], output).Visit(parser.Predicate.Body),
                            onSuccess(remainingInput, output),
                            onFailure(Expression.Constant("Assertion failed"))),
                    onFailure);
            }

            public Expression Visit(AnyChar parser)
            {
                var firstChar = Expression.MakeIndex(input, typeof(IndexedString).GetProperty("Item"), new[] { Expression.Constant(0) });

                var test = Expression.IsFalse(Expression.Property(input, "IsEmpty"));
                var then = onSuccess(Expression.Call(input, "Shift", Type.EmptyTypes, Expression.Constant(1)), firstChar);
                var @else = onFailure(Expression.Constant("Unexpected EOF"));

                return Expression.IfThenElse(test, then, @else);
            }

            public Expression Visit(MatchChar parser)
            {
                var firstChar = Expression.MakeIndex(input, typeof(IndexedString).GetProperty("Item"), new[] { Expression.Constant(0) });

                var test = Expression.AndAlso(
                    Expression.IsFalse(Expression.Property(input, "IsEmpty")),
                    Expression.Equal(firstChar, Expression.Constant(parser.Char)));
                var then = onSuccess(Expression.Call(input, "Shift", Type.EmptyTypes, Expression.Constant(1)), Expression.Constant(parser.Char));
                var @else = onFailure(Expression.Constant(string.Format("Expected '{0}'", parser.Char)));

                return Expression.IfThenElse(test, then, @else);
            }

            public Expression Visit(MatchString parser)
            {
                var initialPart = Expression.Call(input, "Substring", Type.EmptyTypes, new[] { Expression.Constant(0), Expression.Constant(parser.String.Length) });

                var test = Expression.AndAlso(
                    Expression.GreaterThanOrEqual(Expression.Property(input, "Length"), Expression.Constant(parser.String.Length)),
                    Expression.Equal(initialPart, Expression.Constant(parser.String)));
                var then = onSuccess(Expression.Call(input, "Shift", Type.EmptyTypes, Expression.Constant(parser.String.Length)), Expression.Constant(parser.String));
                var @else = onFailure(Expression.Constant(string.Format("Expected '{0}'", parser.String)));

                return Expression.IfThenElse(test, then, @else);
            }

            public Expression Visit<T>(Star<T> parser)
            {
                var brk = Expression.Label();

                var input1 = Expression.Variable(typeof(IndexedString));
                var results = Expression.Variable(typeof(ICollection<T>));

                var body = Compile(parser.Parser, input1, parsers,
                    (remainingInput, output) => Expression.Block(
                        Expression.Call(results, "Add", Type.EmptyTypes, output),
                        Expression.Assign(input1, remainingInput)),
                    _ => Expression.Break(brk));

                return Expression.Block(
                    new ParameterExpression[] { input1, results },
                    new Expression[] 
                    {
                        Expression.Assign(input1, input),
                        Expression.Assign(results, Expression.New(typeof(List<T>))),
                        Expression.Loop(body, brk), 
                        onSuccess(input1, Expression.Call(typeof(Enumerable), "ToArray", new[] { typeof(T) }, results))
                    });
            }

            public Expression Visit<T>(Or<T> parser)
            {
                return Compile(parser.Left, input, parsers,
                    onSuccess,
                    errorMessage1 => Compile(parser.Right, input, parsers, onSuccess,
                        errorMessage2 => onFailure(CombineErrorMessages(errorMessage1, errorMessage2)))
                    );
            }

            private Expression CombineErrorMessages(Expression errorMessage1, Expression errorMessage2)
            {
                return Expression.Call(typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) }),
                    Expression.Constant("Failure at disjunction: {0}, or {1}."),
                    errorMessage1, errorMessage2);
            }

            public Expression Visit<T>(Named<T> parser)
            {
                var compiledParser = Expression.TypeAs(
                    Expression.MakeIndex(parsers,
                        typeof(IDictionary<string, Delegate>).GetProperty("Item"),
                        new[] { Expression.Constant(parser.Name) }),
                    typeof(CompiledParser<T>));

                var result = Expression.Variable(typeof(ParseResult<T>), "result");

                return Expression.Block(new[] { result }, 
                    new Expression [] 
                    {
                        Expression.Assign(result, Expression.Invoke(compiledParser, input, parsers)),
                        Expression.IfThenElse(
                            Expression.Property(result, "Success"),
                            onSuccess(Expression.Property(result, "RemainingInput"), Expression.Property(result, "Output")),
                            onFailure(Expression.Property(result, "ErrorMessage")))
                    });
            }
        }
    }
}
