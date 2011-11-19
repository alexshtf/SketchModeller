using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Utils
{
    /// <summary>
    /// Static class with helper methods concerning expressions of type <see cref="Expression"/>
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Converts an expression into a <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>The member info.</returns>
        public static MemberInfo GetMemberInfo(this Expression expression)
        {
            var memberExpression = MemberExpressionFromLambda(expression);
            return memberExpression.Member;
        }

        public static Tuple<object, MemberInfo> GetTargetAndMember(this Expression expression)
        {
            var memberExpression = MemberExpressionFromLambda(expression);
            var targetExpression = Expression.Lambda<Func<object>>(memberExpression.Expression);
            var targetGetter = targetExpression.Compile();
            var target = targetGetter();
            return Tuple.Create(target, memberExpression.Member);
        }

        private static MemberExpression MemberExpressionFromLambda(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;
            return memberExpression;
        }
    }
}
