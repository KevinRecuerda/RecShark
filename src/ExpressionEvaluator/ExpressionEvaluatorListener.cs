using System.Collections.Generic;
using ExpressionEvaluator.Generated;

namespace RecShark.ExpressionEvaluator
{
    public class ExpressionEvaluatorListener : ExpressionEvaluatorBaseListener
    {
        public HashSet<string> Variables { get; }

        public ExpressionEvaluatorListener()
        {
            this.Variables = new HashSet<string>();
        }

        public override void EnterVariable(ExpressionEvaluatorParser.VariableContext context)
        {
            this.Variables.Add(context.GetText());
            base.EnterVariable(context);
        } 
    }
}