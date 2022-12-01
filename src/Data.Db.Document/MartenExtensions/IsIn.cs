using System;
using System.Linq;
using System.Linq.Expressions;
using Marten;
using Marten.Linq.Parsing;
using Marten.Linq.Fields;
using Weasel.Postgresql.SqlGeneration;
using RecShark.Extensions;

namespace RecShark.Data.Db.Document.MartenExtensions
{


    public class IsIn : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(RangeExtensions.IsIn);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).RawLocator;
            var arr     = expression.Arguments[1].Value();

            if (!(arr is Array array) || array.Length == 0)
                return new WhereFragment("1=1");

            if (array.GetType().GetElementType()?.IsEnum == true)
                arr = array.Cast<object>().Select(x => x.ToString()).ToArray();

            return new WhereFragment($"{locator} = ANY (?)", arr);
        }
    }
}
