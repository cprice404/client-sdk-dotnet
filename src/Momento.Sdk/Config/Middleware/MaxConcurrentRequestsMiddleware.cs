//using System;
//using System.Threading.Tasks;
//using Grpc.Core.Interceptors;

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
            //Console.WriteLine("WithMaxConcurrentRequests waiting for semaphore");
            await _semaphore.WaitOne();
            //Console.WriteLine("WithMaxConcurrentRequests acquired semaphore");
            try
            {
                //Console.WriteLine("MaxConc with semaphore awaiting continuation");
                var result = await continuation(request, callOptions);
                //Console.WriteLine("MaxConc with semaphore returned from continuation, awaiting reponse");
                // ensure that we don't return (and release the semaphore) until the response task is complete
                await result.ResponseAsync;
                //Console.WriteLine("MaxConc with semaphore done awaiting reponse");
                return result;
            }
            finally
            {
                //Console.WriteLine("WithMaxConcurrentRequests releasing semaphore");
                await _semaphore.Release();
            }
        }
    }

    //    public class MaxConcurrentRequestsMiddleware : IMiddleware
    //    {
    //        private readonly FairAsyncSemaphore _semaphore;

    //        public MaxConcurrentRequestsMiddleware(int maxConcurrentRequests)
    //        {
    //            _semaphore = new FairAsyncSemaphore(maxConcurrentRequests);
    //        }

    //        public MiddlewareResponseState<TResponse> WrapRequest<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Func<TRequest, ClientInterceptorContext<TRequest, TResponse>, MiddlewareResponseState<TResponse>> continuation)
    //            where TRequest : class
    //            where TResponse : class
    //        {
    //            Console.WriteLine($"SEMAPHORE MIDDLEWARE WAITING FOR TICKET: {request.GetType()}");
    //            _semaphore.WaitOne().RunSynchronously();

    //            var nextState = continuation(request, context);

    //            var response = nextState.ResponseAsync.ContinueWith(async r =>
    //            {
    //                Console.WriteLine($"SEMAPHORE MIDDLEWARE RELEASING TICKET: {request.GetType()}");
    //                await _semaphore.Release();
    //                return r.Result;
    //            }).Unwrap();

    //            return new MiddlewareResponseState<TResponse>(
    //                ResponseAsync: response,
    //                ResponseHeadersAsync: nextState.ResponseHeadersAsync,
    //                GetStatus: nextState.GetStatus,
    //                GetTrailers: nextState.GetTrailers
    //            );
    //        }
    //    }
}

