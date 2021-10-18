using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Extensions.DependencyInjection.Testing;
using RecShark.Testing.SpecFlow;
using TechTalk.SpecFlow;

namespace RecShark.Extensions.DependencyInjection.Tests.SpecFlow
{
    [Binding]
    public class ProjectSteps : IntegrationSteps<ProjectHooks>
    {
        public ProjectSteps(ProjectHooks hooks) : base(hooks) { }

        [When(@"I solve hooks")]
        public void WhenISolveHooks()
        {
            // done by HooksSolver at start
        }

        [Then(@"the result should be ""(.*)""")]
        public void ThenTheResultShouldBe(string sentence)
        {
            var sample = Hooks.Provider.GetService<ISample>();
            sample.Hello().Should().Be(sentence);
        }
    }

    public class ProjectHooks : DomainHooks
    {
        public ProjectHooks() : base(new ProjectModule()) { }
    }

    public class ProjectModule : DIModule
    {
        public override DIModule[] Dependencies => new DIModule[] {new SharedModule()};

        public override void Load(IServiceCollection services) { }
    }
}
