using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ExpressionEvaluator.Generated;
using FluentAssertions;
using Xunit;

namespace RecShark.ExpressionEvaluator.Tests
{
    public class ExpressionEvaluatorVisitorTests
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("1.5", 1.5)]
        [InlineData("(1)", 1)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData(@"""OK""", @"OK")]
        public void Should_recognize_atom(string expression, object expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("-1", -1)]
        [InlineData("2+3", 5)]
        [InlineData("2-3", -1)]
        [InlineData("2*3", 6)]
        [InlineData("2/3", 0.6667)]
        [InlineData("2^3", 8)]
        [InlineData("3²", 9)]
        [InlineData("5%2", 1)]
        public void Should_evaluate_simple_operation(string expression, double expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().BeApproximately(expected, 0.001);
        }

        [Theory]
        [InlineData("2/0", double.PositiveInfinity)]
        [InlineData("-2/0", double.NegativeInfinity)]
        [InlineData("0/0", double.NaN)]
        public void Should_evaluate_special_operation(string expression, double expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(expected);
        }

        [Theory]
        [InlineData("1 / 2 * 3", 1.5)]
        [InlineData("3 / 3 / 2", 0.5)]
        [InlineData("2 ^ 2 ^ 3", 64)]
        [InlineData("40 + 10 - (2*40) + (100/40) + 0.2", -27.3)]
        [InlineData("3 + 2 * 5", 13)]
        [InlineData("-3²", -9)]
        [InlineData("-3^2", -9)]
        [InlineData("(-3)²", 9)]
        public void Should_manage_operator_priority(string expression, double expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().BeApproximately(expected, 0.001);
        }

        [Theory]
        [InlineData("1 / 2", 0.5)]
        [InlineData(@""""" / 2", 0)]
        public void Should_convert_operation_to_double(string expression, double expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(expected);
        }

        [Fact]
        public void Should_throw_evaluation_exception_When_cant_convert_operation_to_double()
        {
            var tree = ParseTree(@"""string"" / 2");
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage(@"Cannot convert 'string' to double (for '""string"" / 2')");
        }

        [Theory]
        [InlineData("true && true", true)]
        [InlineData(@"true && """"", false)]
        public void Should_convert_condition_to_bool(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_throw_evaluation_exception_When_cant_convert_condition_to_bool()
        {
            var tree = ParseTree(@"""A"" && true");
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage(@"Cannot convert 'A' to bool (for '""A"" && true')");
        }

        [Theory]
        [InlineData(@"""AAA"" =~ 3", "3")]
        public void Should_throw_evaluation_exception_When_cant_convert_condition_parameter_to_string(string expression, string problem)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage($@"Cannot convert '{problem}' to string (for '{expression}')");
        }

        [Theory]
        [InlineData("2 * -3", -6)]
        public void Should_manage_unary_minus(string expression, double expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(expected);
        }

        [Theory]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        public void Should_manage_not(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("2.5 < 3", true)]
        [InlineData("2.5 <= 3", true)]
        [InlineData("2.5 > 3", false)]
        [InlineData("2.5 >= 3", false)]
        public void Should_manage_relational_operators(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("2.5 == 3", false)]
        [InlineData("2.5 != 3", true)]
        [InlineData("true == false", false)]
        [InlineData("true == true", true)]
        [InlineData(@"""OK"" == ""OK""", true)]
        [InlineData(@"""OK"" == ""OK  """, false)]
        public void Should_manage_equality_operators(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("true  && true", true)]
        [InlineData("true  && false", false)]
        [InlineData("false && true", false)]
        [InlineData("false && false", false)]
        [InlineData("true  || true", true)]
        [InlineData("true  || false", true)]
        [InlineData("false || true", true)]
        [InlineData("false || false", false)]
        [InlineData("2.5 > 3 || 2.5 <= 2.5", true)]
        [InlineData("2.5 > 3 || 2.5 < 2.5", false)]
        public void Should_manage_logical_operators(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("false && x", false)]
        [InlineData("true || x", true)]
        public void Should_not_evaluate_right_part_of_logical_operators_when_its_not_necessary(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(@"""Match"" =~ ""tch""", true)]
        [InlineData(@"""Match"" !=~ ""tch""", false)]
        [InlineData(@"""123 Axxxxy"" =~ ""A*y""", true)]
        [InlineData(@"""123 Axxxxy"" !=~ ""A*y""", false)]
        public void Should_manage_matching_operators(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(@"""Value"" in (""a"", ""b"")", false)]
        [InlineData(@"""Value"" in (""a"", ""b"", ""Value"")", true)]
        [InlineData(@"""Value"" not in (""a"", ""b"")", true)]
        [InlineData(@"""Value"" not in (""a"", ""b"", ""Value"")", false)]
        [InlineData(@"5 in (1, 2)", false)]
        [InlineData(@"5 in (1, 2, ""5"")", false)]
        [InlineData(@"5 in (1, 2, 5)", true)]
        public void Should_manage_in_operator(string expression, bool expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(@"true ? 1 : 0", 1d)]
        [InlineData(@"false ? 1 : 0", 0d)]
        [InlineData(@"1==1 ? ""Ok"" : ""Error""", "Ok")]
        [InlineData(@"false ? 2 : false ? 1 : 0", 0d)]
        [InlineData(@"false ? 2 : true ? 1 : 0", 1d)]
        [InlineData(@"true ? 2 : false ? 1 : 0", 2d)]
        [InlineData(@"true ? 2 : true ? 1 : 0", 2d)]
        public void Should_manage_ternary_condition(string expression, object expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("PI + 3", 6.14159)]
        public void Should_manage_constant(string expression, double expected)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().BeApproximately(expected, 0.001);
        }

        [Theory]
        [InlineData("abs(3)", 3)]
        [InlineData("abs(-3)", 3)]
        [InlineData("sign(3)", 1)]
        [InlineData("sign(-3)", -1)]
        [InlineData("sign(2* (-5))", -1)]
        [InlineData("min(-3, 3)", -3)]
        [InlineData("min(-3 , 3, -15)", -15)]
        [InlineData("max(-3, 3)", 3)]
        [InlineData("max(-3 , 3, 15)", 15)]
        [InlineData("sqrt(25)", 5)]
        [InlineData("sqrt(-1)", double.NaN)]
        public void Should_manage_functions(string expression, double expected)
        {
            var tree = ParseTree(expression);

            var visitor = new ExpressionEvaluatorVisitor();

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(expected);
        }

        [Fact]
        public void Should_throw_evaluation_exception_When_unknown_function()
        {
            var tree = ParseTree("func(1)");
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage("Cannot find function 'func'");
        }

        [Fact]
        public void Should_throw_evaluation_exception_When_oneParameterFunction_contains_multiple_parameters()
        {
            var tree = ParseTree("ABS(-3, 3)");
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage("Function 'ABS' accepts only 1 parameter (2 parameters found)");
        }

        [Fact]
        public void Should_throw_evaluation_exception_When_multipleParametersFunction_contains_only_one_parameter()
        {
            var tree = ParseTree("MIN(3)");
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage("Function 'MIN' accepts at least 2 parameters (1 parameter found)");
        }

        [Fact]
        public void Should_manage_one_variable()
        {
            var tree = ParseTree("x + 1");
            var parameters = new Dictionary<string, object> {["x"] = 3};
            var visitor = new ExpressionEvaluatorVisitor(parameters);

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(4);
        }

        [Fact]
        public void Should_throw_evaluation_exception_Having_unknown_variable()
        {
            var tree = ParseTree("x + 1");
            var visitor = new ExpressionEvaluatorVisitor();

            Action action = () => visitor.Visit(tree);

            action.Should().Throw<EvaluationException>().WithMessage("Cannot find variable 'x'");
        }

        [Fact]
        public void Should_use_default_value_Having_unknown_variable_and_ignoring_missing_variables()
        {
            var tree = ParseTree("x");
            var option = new EvaluatorOption() {IgnoreMissingVariables = true};
            var visitor = new ExpressionEvaluatorVisitor(null, option);

            var actual = visitor.Visit(tree);

            actual.Should().Be("");
        }

        [Fact]
        public void Should_manage_multiple_occurence_of_same_variable()
        {
            var tree = ParseTree("x^2 + x + 1");
            var parameters = new Dictionary<string, object> {["x"] = 3};
            var visitor = new ExpressionEvaluatorVisitor(parameters);

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(13);
        }

        [Fact]
        public void Should_manage_multiple_variables()
        {
            var tree = ParseTree("x^2 + y^2");
            var parameters = new Dictionary<string, object> {["x"] = 3, ["y"] = 4};
            var visitor = new ExpressionEvaluatorVisitor(parameters);

            var actual = visitor.Visit(tree);

            actual.AsDouble().Should().Be(25);
        }

        [Fact]
        public void Should_manage_default_variable_for_equality()
        {
            var parameters = new Dictionary<string, object> {["x"] = 0d};
            Visit("x != \"\"", parameters).AsBool().Should().Be(true);
            Visit("x != 0", parameters).AsBool().Should().Be(false);

            parameters["x"] = "";
            Visit("x != \"\"", parameters).AsBool().Should().Be(false);
            Visit("x != 0", parameters).AsBool().Should().Be(true);
        }

        [Fact]
        public void Should_manage_default_variable_for_in()
        {
            var parameters = new Dictionary<string, object> {["x"] = 0d};
            Visit("x not in (\"\")", parameters).AsBool().Should().Be(true);
            Visit("x not in (0)", parameters).AsBool().Should().Be(false);

            parameters["x"] = "";
            Visit("x not in (\"\")", parameters).AsBool().Should().Be(false);
            Visit("x not in (0)", parameters).AsBool().Should().Be(true);
        }

        [Theory]
        [InlineData(0)]
        [InlineData("")]
        public void Should_manage_default_variable_for_relational(object x)
        {
            var parameters = new Dictionary<string, object> {["x"] = x};

            Visit("x > 0", parameters).AsBool().Should().Be(false);
        }

        private static object Visit(string expression, Dictionary<string, object> parameters = null)
        {
            var tree = ParseTree(expression);
            var visitor = new ExpressionEvaluatorVisitor(parameters);

            var result = visitor.Visit(tree);
            return result;
        }

        private static IParseTree ParseTree(string expression)
        {
            var input = new AntlrInputStream(expression);
            var lexer = new ExpressionEvaluatorLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ExpressionEvaluatorParser(tokens);

            IParseTree tree = parser.expr();

            Trace.WriteLine(tree.ToStringTree(parser));

            return tree;
        }
    }
}