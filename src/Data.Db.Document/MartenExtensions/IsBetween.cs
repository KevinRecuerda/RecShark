using System.Linq.Expressions;
using Marten;
using Marten.Linq;
using Marten.Linq.Parsing;
using Marten.Schema;
using RecShark.Extensions;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class IsBetween : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(DateTimeExtensions.IsBetween);
        }

        public IWhereFragment Parse(IQueryableDocument mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).SqlLocator;
            var from    = expression.Arguments[1].Value();
            var to      = expression.Arguments[2].Value();

            if (from != null && to != null)
                return new WhereFragment($"{locator} BETWEEN ? AND ?", from, to);
            if (from != null)
                return new WhereFragment($"{locator} >= ?", from);
            if (to != null)
                return new WhereFragment($"{locator} <= ?", to);

            return new WhereFragment("1=1");
        }
    }
}