namespace RecShark.Extensions.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Xunit;

    public class PropertyExtensionsTests
    {
        [Fact]
        public void GetPropertyValue__Should_return_property_value()
        {
            var animal = new Animal() {Name = "cat"};

            var actual = animal.GetPropertyValue("Name");

            actual.Should().Be("cat");
        }

        [Fact]
        public void GetPropertyValue__Should_return_null_When_unknown_propertyName()
        {
            var animal = new Animal() {Name = "cat"};

            var actual = animal.GetPropertyValue("unknown");

            actual.Should().Be(null);
        }

        [Fact]
        public void GetPropertyValues__Should_return_property_values()
        {
            var animal = new Animal() {Name = "cat", Age = 10};

            var actual = animal.ToKeyValues();

            actual.Should().HaveCount(2);
            actual.Should().Contain(new KeyValuePair<string, object>("Name", "cat"));
            actual.Should().Contain(new KeyValuePair<string, object>("Age",  10));
        }

        [Fact]
        public void ToInfo__Should_display_item_info()
        {
            // Arrange
            var animal = new Animal() {Name = "cat", Age = 10};

            // Act
            var actual = animal.ToInfo();

            // Assert
            var expected = @"
Name = 'cat'
Age  = '10'"
               .Remove(0, Environment.NewLine.Length);
            actual.Should().Be(expected);
        }

        [Fact]
        public void ToInfo__Should_display_item_info_for_primitive()
        {
            var actual = 5.ToInfo();

            actual.Should().Be("5");
        }

        [Fact]
        public void ToAccessor__Should_return_property_values()
        {
            // Arrange
            var animal      = new Animal {Name = "cat", Age = 10};
            var animalEmpty = new Animal();

            // Act
            var actualAnimal = animal.ToAccessor(a => a);
            var actualName   = animal.ToAccessor(a => a.Name);
            var actualAge    = animal.ToAccessor(a => a.Age);
            var actualEmpty  = animalEmpty.ToAccessor(a => a.Name);

            // Assert
            actualAnimal.Get().Should().Be(animal);
            actualName.Get().Should().Be("cat");
            actualName.ToString().Should().Be("cat");
            animal.Name = "dog";
            actualName.Get().Should().Be("dog");
            actualName.ToString().Should().Be("dog");
            actualAge.Get().Should().Be(10);
            actualAge.ToString().Should().Be("10");
            actualEmpty.ToString().Should().BeNull();
            actualEmpty.Get().Should().BeNull();
        }

        private class Animal
        {
            public string Name { get; set; }
            public int    Age  { get; set; }
        }
    }
}