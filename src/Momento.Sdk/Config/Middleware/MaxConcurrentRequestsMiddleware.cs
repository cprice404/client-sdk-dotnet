using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Momento.Sdk.Config.Middleware
{
    public class MaxConcurrentRequestsMiddleware : IMiddleware
    {
        private readonly FairAsyncSemaphore _semaphore;

        public MaxConcurrentRequestsMiddleware(int maxConcurrentRequests)
        {
            _semaphore = new FairAsyncSemaphore(maxConcurrentRequests);
        }

        public async Task<MiddlewareResponseState<TResponse>> WrapRequest<TRequest, TResponse>(TRequest request, CallOptions callOptions, Func<TRequest, CallOptions, Task<MiddlewareResponseState<TResponse>>> continuation)
        {
            await _semaphore.WaitOne();
            try
            {
                var result = await continuation(request, callOptions);
                // ensure that we don't return (and release the semaphore) until the response task is complete
                await result.ResponseAsync;
                return result;
            }
            finally
            {
                await _semaphore.Release();
            }
        }
    }
}

