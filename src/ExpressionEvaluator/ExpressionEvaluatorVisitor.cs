using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using RecShark.ExpressionEvaluator.Extensions;
using RecShark.ExpressionEvaluator.Generated;

namespace RecShark.ExpressionEvaluator
{
    public class ExpressionEvaluatorVisitor : ExpressionEvaluatorBaseVisitor<object>
    {
        private static readonly Dictionary<string, Func<double, double>>              SingleParameterFunctionsByName;
        private static readonly Dictionary<string, Func<IEnumerable<double>, double>> MultipleParametersFunctionsByName;

        private readonly EvaluatorOption option;

        static ExpressionEvaluatorVisitor()
        {
            SingleParameterFunctionsByName = new Dictionary<string, Func<double, double>>()
            {
                {"abs", Math.Abs},
                {"sign", p => Math.Sign(p)},
                {"sqrt", Math.Sqrt}
            };
            MultipleParametersFunctionsByName = new Dictionary<string, Func<IEnumerable<double>, double>>()
            {
                {"min", p => p.Min()},
                {"max", p => p.Max()}
            };
        }

        public ExpressionEvaluatorVisitor(Dictionary<string, object> parameters = null, EvaluatorOption option = null)
        {
            Parameters = parameters ?? new Dictionary<string, object>();

            this.option = option ?? new EvaluatorOption();
        }

        public Dictionary<string, object> Parameters { get; }

        public override object VisitSafeExpr(ExpressionEvaluatorParser.SafeExprContext context)
        {
            // Just visit expr, ignoring <EOF>
            return Visit(context.expr());
        }

        #region Operations

        public override object VisitPowerExpr(ExpressionEvaluatorParser.PowerExprContext context)
        {
            return VisitBinaryOperation(context, ExpressionEvaluatorParser.POW);
        }

        public override object VisitSquareExpr(ExpressionEvaluatorParser.SquareExprContext context)
        {
            var value = Visit(context.expr()).AsDouble(context);
            return value * value;
        }

        public override object VisitChangeSignExpr(ExpressionEvaluatorParser.ChangeSignExprContext context)
        {
            return -1 * Visit(context.expr()).AsDouble(context);
        }

        public override object VisitNotExpr(ExpressionEvaluatorParser.NotExprContext context)
        {
            return !Visit(context.expr()).AsBool(context);
        }

        public override object VisitMultOrDivOrModExpr(ExpressionEvaluatorParser.MultOrDivOrModExprContext context)
        {
            return VisitBinaryOperation(context, context.op.Type);
        }

        public override object VisitPlusOrMinusExpr(ExpressionEvaluatorParser.PlusOrMinusExprContext context)
        {
            return VisitBinaryOperation(context, context.op.Type);
        }

        public override object VisitRelationalExpr(ExpressionEvaluatorParser.RelationalExprContext context)
        {
            return VisitRelationalExpr(context, context.op.Type);
        }

        public override object VisitMatchingExpr(ExpressionEvaluatorParser.MatchingExprContext context)
        {
            return VisitMatchingExpr(context, context.op.Type);
        }

        public override object VisitEqualityExpr(ExpressionEvaluatorParser.EqualityExprContext context)
        {
            return VisitEqualityExpr(context, context.op.Type);
        }

        public override object VisitInExpr(ExpressionEvaluatorParser.InExprContext context)
        {
            return VisitInExpr(context, context.op.Type);
        }

        public override object VisitAndExpr(ExpressionEvaluatorParser.AndExprContext context)
        {
            return VisitConditionalExpr(context, ExpressionEvaluatorParser.AND);
        }

        public override object VisitOrExpr(ExpressionEvaluatorParser.OrExprContext context)
        {
            return VisitConditionalExpr(context, ExpressionEvaluatorParser.OR);
        }

        private double VisitBinaryOperation(ParserRuleContext context, int op)
        {
            var left  = this.WalkLeft(context).AsDouble(context);
            var right = this.WalkRight(context).AsDouble(context);

            switch (op)
            {
                case ExpressionEvaluatorParser.PLUS:
                    return left + right;
                case ExpressionEvaluatorParser.MINUS:
                    return left - right;
                case ExpressionEvaluatorParser.MULT:
                    return left * right;
                case ExpressionEvaluatorParser.DIV:
                    return left / right;
                case ExpressionEvaluatorParser.MOD:
                    return left % right;
                case ExpressionEvaluatorParser.POW:
                    return Math.Pow(left, right);
                default:
                    throw new EvaluationException("Invalid arithmetic operator !");
            }
        }

        private bool VisitRelationalExpr(ParserRuleContext context, int op)
        {
            var left  = this.WalkLeft(context).AsDouble(context);
            var right = this.WalkRight(context).AsDouble(context);

            switch (op)
            {
                case ExpressionEvaluatorParser.GE:
                    return left >= right;
                case ExpressionEvaluatorParser.GT:
                    return left > right;
                case ExpressionEvaluatorParser.LE:
                    return left <= right;
                case ExpressionEvaluatorParser.LT:
                    return left < right;
                default:
                    throw new EvaluationException("Invalid relational operator !");
            }
        }

        private bool VisitEqualityExpr(ParserRuleContext context, int op)
        {
            var left  = this.WalkLeft(context);
            var right = this.WalkRight(context);

            var areEquals = left.Equals(right);
            switch (op)
            {
                case ExpressionEvaluatorParser.EQ:
                    return areEquals;
                case ExpressionEvaluatorParser.NE:
                    return !areEquals;
                default:
                    throw new EvaluationException("Invalid equality operator !");
            }
        }

        private bool VisitMatchingExpr(ParserRuleContext context, int op)
        {
            var left  = this.WalkLeft(context).AsString(context);
            var right = this.WalkRight(context).AsString(context);

            var regex      = new Regex(right);
            var isMatching = regex.Match(left).Success;
            switch (op)
            {
                case ExpressionEvaluatorParser.MATCH:
                    return isMatching;
                case ExpressionEvaluatorParser.NOMATCH:
                    return !isMatching;
                default:
                    throw new EvaluationException("Invalid matching operator !");
            }
        }

        private bool VisitInExpr(ParserRuleContext context, int op)
        {
            var left       = this.WalkLeft(context);
            var parameters = this.WalkRight(context).AsList(context);

            var isMatching = parameters.Any(parameter => left.Equals(parameter));
            switch (op)
            {
                case ExpressionEvaluatorParser.IN:
                    return isMatching;
                case ExpressionEvaluatorParser.NOTIN:
                    return !isMatching;
                default:
                    throw new EvaluationException("Invalid in operator !");
            }
        }

        private bool VisitConditionalExpr(ParserRuleContext context, int op)
        {
            bool Left() => this.WalkLeft(context).AsBool(context);

            bool Right() => this.WalkRight(context).AsBool(context);

            switch (op)
            {
                case ExpressionEvaluatorParser.AND:
                    return Left() && Right();
                case ExpressionEvaluatorParser.OR:
                    return Left() || Right();
                default:
                    throw new EvaluationException("Invalid conditional operator !");
            }
        }

        #endregion

        #region Ternary condition

        public override object VisitTernaryExpr(ExpressionEvaluatorParser.TernaryExprContext context)
        {
            var condition   = this.WalkLeft(context).AsBool(context);
            var valueIfTrue = this.WalkRight(context);

            return condition ? valueIfTrue : this.Walk(context, 2);
        }

        #endregion

        #region Function

        public override object VisitFunction(ExpressionEvaluatorParser.FunctionContext context)
        {
            var functionName = context.funcName().GetText();
            var parameters = this.WalkRight(context)
                                 .AsList(context)
                                 .Select(p => p.AsDouble(context))
                                 .ToList();

            if (SingleParameterFunctionsByName.TryGetValue(functionName.ToLower(), out var singleParameterFunction))
            {
                if (parameters.Count != 1)
                    throw new EvaluationException(
                        $"Function '{functionName}' accepts only 1 parameter ({parameters.Count} parameters found)");

                return singleParameterFunction(parameters.First());
            }

            if (MultipleParametersFunctionsByName.TryGetValue(functionName.ToLower(), out var multipleParametersFunction))
            {
                if (parameters.Count < 2)
                    throw new EvaluationException(
                        $"Function '{functionName}' accepts at least 2 parameters ({parameters.Count} parameter found)");

                return multipleParametersFunction(parameters);
            }

            throw new EvaluationException($"Cannot find function '{functionName}'");
        }

        public override object VisitParameters(ExpressionEvaluatorParser.ParametersContext context)
        {
            return context.expr().Select(Visit).ToList();
        }

        #endregion

        #region Atom

        public override object VisitBraces(ExpressionEvaluatorParser.BracesContext context)
        {
            return Visit(context.expr());
        }

        public override object VisitNumber(ExpressionEvaluatorParser.NumberContext context)
        {
            return Convert.ToDouble(context.num().GetText(), CultureInfo.InvariantCulture);
        }

        public override object VisitBoolean(ExpressionEvaluatorParser.BooleanContext context)
        {
            return Convert.ToBoolean(context.@bool().GetText().ToLower());
        }

        public override object VisitVariable(ExpressionEvaluatorParser.VariableContext context)
        {
            var variableName = context.var().GetText();
            if (Parameters.TryGetValue(variableName, out var value))
                return value;

            if (option?.IgnoreMissingVariables == true)
                return ObjectExtensions.Default;

            throw new EvaluationException($"Cannot find variable '{variableName}'");
        }

        public override object VisitString(ExpressionEvaluatorParser.StringContext context)
        {
            var str = context.str().GetText();

            // Remove quote
            str = str.Substring(1, str.Length - 2).Replace("\"\"", "\"");

            return str;
        }

        #endregion

        #region Constants

        public override object VisitConstPi(ExpressionEvaluatorParser.ConstPiContext context)
        {
            return Math.PI;
        }

        #endregion
    }
}