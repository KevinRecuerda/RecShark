using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RecShark.Extensions
{
    public static class Watch
    {
        public static async Task<long> Ms(Func<Task> action)
        {
            var sw = new Stopwatch();
            sw.Start();
            await action();
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }

        public static long Ms(Action action)
        {
            Task ActionAsync()
            {
                action();
                return Task.CompletedTask;
            }

            return Ms(ActionAsync).Result;
        }
    }
}
