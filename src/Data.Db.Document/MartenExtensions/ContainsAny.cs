using System.Linq;
using System.Linq.Expressions;
using Marten;
using Marten.Linq.Fields;
using Marten.Linq.Parsing;
using RecShark.Extensions;
using Weasel.Postgresql.SqlGeneration;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    

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
