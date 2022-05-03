using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RecShark.DependencyInjection;
using RecShark.Testing.DependencyInjection;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow.Tests.DependencyInjection
{
    [Binding]
    public class ProjectSteps : FunctionalSteps<ProjectHooks>
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
            var sample = Hooks.Provider.GetService<ISampleApiClient>();
            sample.Hello().Should().Be(sentence);
        }
    }

    public class ProjectHooks : FunctionalHooks
    {
        public ProjectHooks() : base(new ProjectModule()) { }
    }

    public class ProjectModule : DIModule
    {
        public override DIModule[] Dependencies => new DIModule[] {new SharedModule()};

        public override void Load(IServiceCollection services) { }
    }
}
