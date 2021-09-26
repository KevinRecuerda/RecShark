using System;

namespace RecShark.ExpressionEvaluator
{
    public class ParsingException : Exception
    {
        public ParsingException(string message) : base(message) { }
    }
}