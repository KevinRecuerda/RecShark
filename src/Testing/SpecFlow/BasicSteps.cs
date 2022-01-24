using RecShark.Extensions;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow
{
    public abstract class BasicSteps : Steps
    {
        static BasicSteps()
        {
            typeof(Tests).RunStaticConstructor();
            SpecFlowExtensions.UseObjectConverter();
        }
    }
}
