 using System.Linq;
 using NSubstitute;
 using NSubstitute.Exceptions;

 namespace RecShark.Extensions.Testing.NSubstitute
{
    public static class CallExtensions
    {
        public static void DidNotReceiveAnyCall<T>(this T substitute)
            where T : class
        {
            var receivedCalls = substitute.DidNotReceive().ReceivedCalls().ToList();
            if (receivedCalls.Any())
                throw new ReceivedCallsException($"Expected to receive no calls, but received {receivedCalls.Count} calls");
        }
    }
}