using Polly;
using System.Threading.Tasks;
using System;
using AVBDelivery.Exceptions;

namespace AVBDelivery.Helpers
{
    public static class PollyPolicyFactory
    {
        public static AsyncPolicy CreateCommonErrorRetryPolicy()
        {
            return Policy
                .Handle<HttpException>(ex => ex.StatusCode != 401)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromMilliseconds(300),
                });
        }
        public static AsyncPolicy CreateAuthRetryPolicy(Func<Task> refreshTokenFunc)
        {
            return Policy
                .Handle<HttpException>(ex => ex.StatusCode == 401)
                .RetryAsync(1, async (_, _) =>
                {
                    Console.WriteLine("Token expired or invalid! Refreshing...");
                    await refreshTokenFunc();
                });
        }
    }
}
