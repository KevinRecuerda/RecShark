using System.Linq.Expressions;
using Marten;
using Marten.Linq.Parsing;
using RecShark.Extensions;
using Marten.Linq.Fields;
using Weasel.Postgresql.SqlGeneration;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class IsBetween : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(DateTimeExtensions.IsBetween);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).RawLocator;
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
