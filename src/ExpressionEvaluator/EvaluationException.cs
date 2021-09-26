using System;

namespace RecShark.ExpressionEvaluator
{
    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message) { }
    }
}