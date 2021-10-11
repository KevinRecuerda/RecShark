using System.Linq;
using System.Linq.Expressions;
using Marten;
using Marten.Linq;
using Marten.Linq.Parsing;
using Marten.Schema;
using RecShark.Extensions;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class ContainsAny : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(EnumerableExtensions.ContainsAny);
        }

        public IWhereFragment Parse(IQueryableDocument mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).SqlLocator;
            var values  = expression.Arguments.Last().Value();

            return new CollectionWhereFragment($"{locator} ?| ??", values);
        }
    }
}