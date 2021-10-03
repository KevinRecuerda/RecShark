using System;
using System.Linq;

namespace RecShark.Extensions
{
    public static class ExceptionExtensions
    {
        public static TException Find<TException>(this Exception exception)
            where TException : Exception
        {
            while (exception != null && !(exception is TException))
                exception = exception.InnerException;
            return exception as TException;
        }
        
        public static Exception Last(this Exception exception)
        {
            while (exception?.InnerException != null)
                exception = exception.InnerException;
            return exception;
        }
        
        public static string SmartMessage(this Exception exception)
        {
            if (exception is AggregateException aggregateException)
                return aggregateException.InnerExceptions.Select(e => e.Message).ToLines();
            return exception.Message;
        }
    }
}