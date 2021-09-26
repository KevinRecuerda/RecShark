using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ExpressionEvaluator.Generated;
using FluentAssertions;
using Xunit;

namespace RecShark.ExpressionEvaluator.Tests
{
    public class ExpressionEvaluatorListenerTests
    {
        [Fact]
        public void Should_find_no_variable()
        {
            var evaluatorListener = CreateTreeAndWalkListener("2 + 3");

            evaluatorListener.Variables.Should().BeEmpty();
        }

        [Fact]
        public void Should_find_one_variable()
        {
            var evaluatorListener = CreateTreeAndWalkListener("2 * x + 3");

            evaluatorListener.Variables.Should().ContainInOrder("x");
        }

        [Fact]
        public void Should_find_several_variables()
        {
            var evaluatorListener = CreateTreeAndWalkListener(@"2 * x + 3 * y + 2 ^ power + IsOk * 5 + (Car = ""Any"") * 3");

            evaluatorListener.Variables.Should().ContainInOrder("x", "y", "power", "IsOk", "Car");
        }

        private static ExpressionEvaluatorListener CreateTreeAndWalkListener(string expression)
        {
            var input = new AntlrInputStream(expression);
            var lexer = new ExpressionEvaluatorLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ExpressionEvaluatorParser(tokens);
            IParseTree tree = parser.expr();

            var evaluatorListener = new ExpressionEvaluatorListener();
            var walker = new ParseTreeWalker();
            walker.Walk(evaluatorListener, tree);

            return evaluatorListener;
        }
    }
}