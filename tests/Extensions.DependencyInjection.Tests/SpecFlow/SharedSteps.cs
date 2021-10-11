using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RecShark.Extensions.DependencyInjection.Testing;
using RecShark.Testing.SpecFlow;
using TechTalk.SpecFlow;

namespace RecShark.Extensions.DependencyInjection.Tests.SpecFlow
{
    [Binding]
    public class SharedSteps : IntegrationSteps<SharedHooks>
    {
        public SharedSteps(SharedHooks hooks) : base(hooks) { }

        [Given(@"a sentence ""(.*)""")]
        public void GivenASentence(string sentence)
        {
            var sample = this.Hooks.Provider.GetService<ISample>();
            sample.Hello().Returns(sentence);
        }
    }

    public class SharedHooks : DomainHooks
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