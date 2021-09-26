using System.Collections.Generic;
using Antlr4.Runtime;

namespace RecShark.ExpressionEvaluator
{
    public class ThrowingErrorListener : BaseErrorListener
    {
        public List<string> Errors { get; }

        public ThrowingErrorListener()
        {
            this.Errors = new List<string>();
        }

        public override void SyntaxError(
            IRecognizer          recognizer,
            IToken               offendingSymbol,
            int                  line,
            int                  charPositionInLine,
            string               msg,
            RecognitionException e)
        {
            this.Errors.Add($"line {line}:{charPositionInLine} {msg}");
        }
    }
}