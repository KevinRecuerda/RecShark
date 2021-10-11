using RecShark.Extensions;
using TechTalk.SpecFlow.Assist.ValueRetrievers;

namespace RecShark.Testing.SpecFlow
{
    public class ObjectValueRetriever : ClassRetriever<object>
    {
        protected override object GetNonEmptyValue(string value)
        {
            return ConvertExtensions.ConvertSmart(value);
        }
    }
}