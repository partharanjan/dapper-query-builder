using DapperQueryBuilder.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DapperQueryBuilder.Helper
{
    internal class LinqHelper<TEntity>
    {
        public ConditionQuery GetQuery(Expression<Func<TEntity, bool>> expression)
        {
            var binaryExpression = expression.Body as BinaryExpression;
            return new ConditionQuery(null, GetColumn(binaryExpression.Left), GetOperator(binaryExpression.NodeType), GetValue(binaryExpression.Right));
        }

        private string GetColumn(Expression leftExpression)
        {
            MemberExpression leftMember = null;
            if (leftExpression.NodeType == ExpressionType.MemberAccess)
            {
                leftMember = leftExpression as MemberExpression;
            }
            else if (leftExpression.NodeType == ExpressionType.Convert)
            {
                var ue = leftExpression as UnaryExpression;
                leftMember = (ue?.Operand) as MemberExpression;
            }

            if (leftMember != null)
            {
                return leftMember.Member.Name;
            }

            throw new Exception("No handler found for left expression");
        }

        private object GetValue(Expression rightExpression)
        {
            if (rightExpression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)rightExpression).Value;
            }
            else if (rightExpression.NodeType == ExpressionType.MemberAccess)
            {
                return Expression.Lambda(rightExpression).Compile().DynamicInvoke();
            }
            throw new Exception("No handler found for right expression");
        }

        private string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal: { return "="; }
                case ExpressionType.NotEqual: { return "!="; }
                case ExpressionType.GreaterThan: { return ">"; }
                case ExpressionType.GreaterThanOrEqual: { return ">="; }
                case ExpressionType.LessThan: { return "<"; }
                case ExpressionType.LessThanOrEqual: { return "<="; }
                default:
                    {
                        throw new Exception("No Operation found.");
                    }
            }
        }
    }
}
