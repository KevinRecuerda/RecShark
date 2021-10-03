using System;

namespace RecShark.Extensions
{
    public static class Ensure
    {
        public static void ArgumentNotNull(object argument, string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name, "Cannot be null");
        }

        public static void ArgumentNotNullOrEmpty(string argument, string name)
        {
            if (argument.IsNullOrEmpty())
                throw new ArgumentException("Cannot be null or empty", name);
        }
    }
}