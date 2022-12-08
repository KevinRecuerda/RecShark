using System;
using System.Linq;
using System.Linq.Expressions;
using Marten;
using Marten.Linq.Fields;
using Marten.Linq.Parsing;
using RecShark.Extensions;
using Weasel.Postgresql.SqlGeneration;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class SmartMatchAny : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(StringExtensions.SmartMatchAny);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).RawLocator;
            var arr     = expression.Arguments[1].Value();

            if (!(arr is Array array) || array.Length == 0)
                return new WhereFragment("1=1");

            arr = array.Cast<string>().Select(x => x.Replace("*", "%")).ToArray();

            return new WhereFragment($"{locator} LIKE ANY (?)", arr);
        }
    }
}
