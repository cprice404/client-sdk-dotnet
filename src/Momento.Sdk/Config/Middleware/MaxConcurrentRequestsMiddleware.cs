//using System;
//using System.Threading.Tasks;
//using Grpc.Core.Interceptors;

//namespace Momento.Sdk.Config.Middleware
//{
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
//}

