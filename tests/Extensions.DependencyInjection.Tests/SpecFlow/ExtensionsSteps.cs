using System.Linq;
using FluentAssertions;
using RecShark.Testing.SpecFlow;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace RecShark.Extensions.DependencyInjection.Tests.SpecFlow
{
    [Binding]
    public class ExtensionsSteps : IntegrationSteps<SharedHooks>
    {
        private ObjectToInstantiate   instance;
        private ObjectToInstantiate[] set;

        [When(@"I create instance from")]
        public void WhenICreateInstanceFrom(Table table)
        {
            this.instance = table.CreateInstance<ObjectToInstantiate>();
        }

        [When(@"I create set from")]
        public void WhenICreateSetFrom(Table table)
        {
            this.set = table.CreateSet<ObjectToInstantiate>().ToArray();
        }

        [Then(@"value should be of type (.*)")]
        public void ThenValueShouldBeOfTypeDouble(string type)
        {
            this.instance.Value.GetType().Name.Should().Be(type);
        }

        [Then(@"value should not be null")]
        public void ThenValueShouldNotBeNull()
        {
            foreach (var item in this.set)
                item.Value.Should().NotBe(null);
        }
    }

    public class ObjectToInstantiate
    {
        public int    Id    { get; set; }
        public object Value { get; set; }
    }
}
