﻿using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using RecShark.ExpressionEvaluator.Extensions;
using RecShark.ExpressionEvaluator.Generated;

namespace RecShark.ExpressionEvaluator
{
    public class Expression
    {
        private readonly EvaluatorOption           option;
        private readonly ExpressionEvaluatorParser parser;
        private readonly IParseTree                tree;

        public Expression(string expression, EvaluatorOption option = null)
        {
            Description = expression;
            this.option      = option ?? new EvaluatorOption();

            parser = CreateParser(expression);
            var errorListener = CreateErrorListener(parser);
            tree = Parse(parser, errorListener);

            Variables = ExtractVariables(tree);
        }

        public string Description { get; }

        public List<string> Variables { get; }

        public object Evaluate(Dictionary<string, object> parameters = null)
        {
            return Evaluate<object>(parameters);
        }

        public T Evaluate<T>(Dictionary<string, object> parameters = null)
        {
            var visitor = new ExpressionEvaluatorVisitor(parameters, option);

            var result = visitor.Visit(tree);

            if (typeof(T) == typeof(object))
                return (T) result;

            if (typeof(T) == typeof(double))
                return (T) (object) result.AsDouble();

            if (typeof(T) == typeof(bool))
                return (T) (object) result.AsBool();

            if (typeof(T) == typeof(string))
                return (T) (object) result.AsString();

            return (T) Convert.ChangeType(result, typeof(T));
        }

        public string ToStringTree()
        {
            return tree.ToStringTree(parser);
        }

        private static ExpressionEvaluatorParser CreateParser(string expression)
        {
            var input  = new AntlrInputStream(expression);
            var lexer  = new ExpressionEvaluatorLexer(input);
            var tokens = new CommonTokenStream(lexer);
            return new ExpressionEvaluatorParser(tokens);
        }

        private static ThrowingErrorListener CreateErrorListener(ExpressionEvaluatorParser evaluatorParser)
        {
            var throwingErrorListener = new ThrowingErrorListener();
            evaluatorParser.AddErrorListener(throwingErrorListener);
            return throwingErrorListener;
        }

        private static IParseTree Parse(ExpressionEvaluatorParser evaluatorParser, ThrowingErrorListener throwingErrorListener)
        {
            IParseTree parseTree = evaluatorParser.safeExpr();

            if (throwingErrorListener.Errors.Any())
                throw new ParsingException(string.Join(Environment.NewLine, throwingErrorListener.Errors));

            return parseTree;
        }

        private static List<string> ExtractVariables(IParseTree parseTree)
        {
            var evaluatorListener = new ExpressionEvaluatorListener();
            var walker            = new ParseTreeWalker();

            walker.Walk(evaluatorListener, parseTree);

            return evaluatorListener.Variables.ToList();
        }
    }
}