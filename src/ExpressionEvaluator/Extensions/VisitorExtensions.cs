using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace RecShark.ExpressionEvaluator.Extensions
{
    public static class VisitorExtensions
    {
        public static T WalkLeft<T>(this IParseTreeVisitor<T> visitor, ParserRuleContext context)
        {
            return visitor.Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public static T WalkRight<T>(this IParseTreeVisitor<T> visitor, ParserRuleContext context)
        {
            return visitor.Visit(context.GetRuleContext<ParserRuleContext>(1));
        }

        public static T Walk<T>(this IParseTreeVisitor<T> visitor, ParserRuleContext context, int n)
        {
            return visitor.Visit(context.GetRuleContext<ParserRuleContext>(n));
        }
    }
}