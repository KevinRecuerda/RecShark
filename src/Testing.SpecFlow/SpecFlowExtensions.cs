using TechTalk.SpecFlow.Assist;

namespace RecShark.Testing.SpecFlow.Extensions
{
    public static class SpecFlowExtensions
    {
        public static void UseObjectConverter()
        {
            Service.Instance.ValueRetrievers.Register(new ObjectValueRetriever());
        }
    }
}
