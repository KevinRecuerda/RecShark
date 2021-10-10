using System;
using System.Linq;
using System.Linq.Expressions;

namespace RecShark.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            return CombineBinaryExpr(Expression.And, left, right);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            return CombineBinaryExpr(Expression.OrElse, left, right);
        }

        public static Expression<Func<T, bool>> CombineBinaryExpr<T>(
            Func<Expression, Expression, BinaryExpression> exprBuilder,
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            if (left == null)
                return right;

            var expr = exprBuilder(left.Body, right.Body);
            return Expression.Lambda<Func<T, bool>>(expr, left.Parameters.Single());
        }
    }
}