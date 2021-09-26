using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RecShark.ExpressionEvaluator.Tests
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void Should_manage_default_value()
        {
            var actual = ObjectExtensions.Default;

            actual.AsDouble().Should().Be(0);
            actual.AsBool().Should().Be(false);
            actual.AsString().Should().Be("");
            actual.AsList().Should().BeEmpty();
        }

        [Fact]
        public void Should_manage_double()
        {
            const double actual = 1.5;

            actual.AsDouble().Should().Be(1.5);
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_default_double()
        {
            const double actual = 0;

            actual.AsDouble().Should().Be(0);
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_int()
        {
            const int actual = 1;

            actual.AsDouble().Should().Be(1);
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_default_int()
        {
            const int actual = 0;

            actual.AsDouble().Should().Be(0);
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_decimal()
        {
            const decimal actual = 1.5M;

            actual.AsDouble().Should().Be(1.5);
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_default_decimal()
        {
            const decimal actual = 0M;

            actual.AsDouble().Should().Be(0);
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_bool()
        {
            const bool actual = true;

            Assert.Throws<EvaluationException>(() => actual.AsDouble());
            actual.AsBool().Should().Be(true);
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_default_bool()
        {
            const bool actual = false;

            Assert.Throws<EvaluationException>(() => actual.AsDouble());
            actual.AsBool().Should().Be(false);
            Assert.Throws<EvaluationException>(() => actual.AsString());
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_string()
        {
            const string actual = "hello";

            Assert.Throws<EvaluationException>(() => actual.AsDouble());
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            actual.AsString().Should().Be("hello");
            Assert.Throws<EvaluationException>(() => actual.AsList());
        }

        [Fact]
        public void Should_manage_empty_string()
        {
            const string actual = "";

            actual.AsDouble().Should().Be(0);
            actual.AsBool().Should().Be(false);
            actual.AsString().Should().Be("");
            actual.AsList().Should().BeEmpty();
        }

        [Fact]
        public void Should_manage_list()
        {
            var actual = new List<object> {1, 2};

            Assert.Throws<EvaluationException>(() => actual.AsDouble());
            Assert.Throws<EvaluationException>(() => actual.AsBool());
            Assert.Throws<EvaluationException>(() => actual.AsString());
            actual.AsList().Should().ContainInOrder(1, 2);
        }
    }
}