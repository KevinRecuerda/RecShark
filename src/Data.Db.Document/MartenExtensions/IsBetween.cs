using System.Linq.Expressions;
using Marten;
using Marten.Linq.Parsing;
using RecShark.Extensions;
using Marten.Linq.Fields;
using Weasel.Postgresql.SqlGeneration;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    using System;

    public class IsBetween : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(DateTimeExtensions.IsBetween);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).TypedLocator;
            var fromInput    = (DateTime?) expression.Arguments[1].Value();
            var toInput      = (DateTime?) expression.Arguments[2].Value();

            var from = fromInput?.Kind == DateTimeKind.Utc ? fromInput.Value.ToLocalTime() : fromInput;
            var to   = toInput?.Kind   == DateTimeKind.Utc ? toInput.Value.ToLocalTime() : toInput;
            
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
