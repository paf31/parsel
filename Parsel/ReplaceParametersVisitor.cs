using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    internal static class ReplaceParameter
    {
        public static Expression Apply<S, T>(this Expression<Func<S, T>> f, Expression s)
        {
            return f.Body.Replace(f.Parameters[0], s);
        }

        public static Expression Apply<S, T, V>(this Expression<Func<S, T, V>> f, Expression s, Expression t)
        {
            return f.Body.Replace(f.Parameters[0], s).Replace(f.Parameters[1], t);
        }

        public static Expression Replace(this Expression body, ParameterExpression parameter, Expression replacement)
        {
            return new ReplaceParameterVisitor(parameter, replacement).Visit(body);
        }
    }

    internal class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression parameter;
        private readonly Expression replacement;

        public ReplaceParameterVisitor(ParameterExpression parameter, Expression replacement)
        {
            this.parameter = parameter;
            this.replacement = replacement;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Equals(parameter))
            {
                return replacement;
            }
            else
            {
                return base.VisitParameter(node);
            }
        }
    }
}
