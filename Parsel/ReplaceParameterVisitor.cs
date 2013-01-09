using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Parsel
{
    public class ReplaceParameterVisitor : ExpressionVisitor
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
