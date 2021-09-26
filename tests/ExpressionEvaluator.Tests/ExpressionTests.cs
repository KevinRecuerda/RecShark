using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RecShark.ExpressionEvaluator.Tests
{
    public class ExpressionTests
    {
        [Fact]
        public void Should_evaluate_operation_as_double()
        {
            var expression = new Expression(@"x*y^2 + (isOk ? 100 : 50) + (car == ""any"" ? 1000 : 500)");

            var parameters = new Dictionary<string, object>
            {
                ["x"] = 3,
                ["y"] = 5,
                ["isOk"] = true,
                ["car"] = "any"
            };

            var actual = expression.Evaluate<double>(parameters);

            actual.Should().Be(75 + 100 + 1000);
        }

        [Fact]
        public void Should_evaluate_condition_as_boolean()
        {
            var expression = new Expression(@"x < y && isOk || car == ""any""");

            var parameters = new Dictionary<string, object>
            {
                ["x"] = 3,
                ["y"] = 5,
                ["isOk"] = true,
                ["car"] = "any"
            };

            var actual = expression.Evaluate<bool>(parameters);

            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_evaluate_condition_as_string()
        {
            var expression = new Expression(@"x < y ? ""true"" : ""false""");

            var parameters = new Dictionary<string, object> {["x"] = 3, ["y"] = 5};
            var actual = expression.Evaluate<string>(parameters);
            actual.Should().Be("true");

            parameters["x"] = 5;
            actual = expression.Evaluate<string>(parameters);
            actual.Should().Be("false");
        }

        [Theory]
        [InlineData(5)]
        [InlineData(true)]
        [InlineData("ok")]
        public void Should_evaluate_multiple_parameter_type(object parameter)
        {
            const string formula = @"1 < 2 ? x : ""error""";

            var parameters = new Dictionary<string, object> {["x"] = parameter};

            var expression = new Expression(formula);
            var actual = expression.Evaluate(parameters);

            actual.Should().Be(parameter);
        }

        [Fact]
        public void Should_evaluate_parameter_value()
        {
            var expression = new Expression("x ^ 2 + y");

            var parameters = new Dictionary<string, object> {["x"] = 3, ["y"] = -0.5};
            var actual = expression.Evaluate<double>(parameters);
            actual.Should().Be(8.5);

            parameters["x"] = 5;
            actual = expression.Evaluate<double>(parameters);
            actual.Should().Be(24.5);
        }

        [Fact]
        public void Should_evaluate_empty_string_as_double_0()
        {
            var expression = new Expression("x");
            var parameters = new Dictionary<string, object> {["x"] = ""};

            var actual = expression.Evaluate<double>(parameters);

            actual.Should().Be(0);
        }

        [Fact]
        public void Should_evaluate_empty_string_as_bool_false()
        {
            var expression = new Expression("x");
            var parameters = new Dictionary<string, object> {["x"] = ""};

            var actual = expression.Evaluate<bool>(parameters);

            actual.Should().BeFalse();
        }

        [Theory]
        [InlineData("(2 + 3", "line 1:6 missing ')' at '<EOF>'")]
        [InlineData("2 + 3)", "line 1:5 extraneous input ')' expecting <EOF>")]
        [InlineData("1 * 2 3", "line 1:6 extraneous input '3' expecting <EOF>")]
        [InlineData("1 *", "line 1:3 mismatched input '<EOF>' expecting {PI, '-', '!', TRUE, FALSE, '(', ID, NUMBER, STRING}")]
        [InlineData("1 && || 0", "line 1:5 extraneous input '||' expecting {PI, '-', '!', TRUE, FALSE, '(', ID, NUMBER, STRING}")]
        public void Should_throw_exception_When_parse_bad_formula(string expression, string expectedMessage)
        {
            Func<Expression> action = () => new Expression(expression);

            action.Should().Throw<ParsingException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void Should_throw_exception_When_evaluate_as_double_and_result_is_not()
        {
            var expression = new Expression("x");
            var parameters = new Dictionary<string, object> {["x"] = "a"};

            Action action = () => expression.Evaluate<double>(parameters);

            action.Should().Throw<EvaluationException>().WithMessage("Cannot convert 'a' to double");
        }

        [Fact]
        public void Should_throw_exception_When_result_is_not_of_requested_type()
        {
            var expression = new Expression("x");
            var parameters = new Dictionary<string, object> {["x"] = "a"};

            Action action = () => expression.Evaluate<DateTime>(parameters);

            action.Should().Throw<FormatException>().WithMessage("The string 'a' was not recognized as a valid DateTime.*");
        }

        [Fact]
        public void Should_fill_description()
        {
            var expression = new Expression("x + y");
            expression.Description.Should().Be("x + y");
        }

        [Fact]
        public void Should_fill_variables()
        {
            var expression = new Expression("x + y");
            expression.Variables.Should().ContainInOrder("x", "y");
        }

        [Fact]
        public void Should_display_tree()
        {
            var expression = new Expression("x + 1");
            expression.ToStringTree().Should().Be("(safeExpr (expr (expr (atom (var x))) + (expr (atom (num 1)))) <EOF>)");
        }
    }
}