using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RecShark.DependencyInjection;
using RecShark.DependencyInjection.Tests;
using RecShark.Testing.DependencyInjection;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow.Tests.DependencyInjection
{
    [Binding]
    public class SharedSteps : FunctionalSteps<SharedHooks>
    {
        public SharedSteps(SharedHooks hooks) : base(hooks) { }

        [Given(@"a sentence ""(.*)""")]
        public void GivenASentence(string sentence)
        {
            var sample = Hooks.Provider.GetService<ISample>();
            sample.Hello().Returns(sentence);
        }
    }

    public class SharedHooks : FunctionalHooks
    {
        public SharedHooks() : base(new SharedModule()) { }
    }

    public class SharedModule : DIModule
    {
        public override void Load(IServiceCollection services)
        {
            services.AddTransient<ISample, SampleBaseApiClient>();
        }
    }
}