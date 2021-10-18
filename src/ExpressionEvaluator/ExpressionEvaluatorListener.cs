using System.Collections.Generic;
using RecShark.ExpressionEvaluator.Generated;

namespace RecShark.ExpressionEvaluator
{
    public class ExpressionEvaluatorListener : ExpressionEvaluatorBaseListener
    {
        public HashSet<string> Variables { get; }

        public ExpressionEvaluatorListener()
        {
            Variables = new HashSet<string>();
        }

        public override void EnterVariable(ExpressionEvaluatorParser.VariableContext context)
        {
            Variables.Add(context.GetText());
            base.EnterVariable(context);
        } 
    }
}