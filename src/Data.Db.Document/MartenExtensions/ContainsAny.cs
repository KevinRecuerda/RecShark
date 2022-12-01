using System.Linq;
using System.Linq.Expressions;
using Marten;
using Marten.Linq;
using Marten.Linq.Parsing;
using Marten.Schema;
using RecShark.Extensions;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    using Marten.Linq.Fields;
    using Weasel.Postgresql.SqlGeneration;

    public class ContainsAny : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(EnumerableExtensions.ContainsAny);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).RawLocator; //SqlLocator; TODO check RawLocator ?
            var values  = expression.Arguments.Last().Value();

            return new CollectionWhereFragment($"{locator} ?| ??", values);
        }
    }
}
