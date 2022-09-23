using System;
using Grpc.Core.Interceptors;

namespace Momento.Sdk.Config.Middleware
{
    public class LoggingMiddleware : IMiddleware
    {
        public MiddlewareRequestState<TResponse> WrapRequest<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Func<TRequest, ClientInterceptorContext<TRequest, TResponse>, MiddlewareRequestState<TResponse>> continuation)
            where TRequest : class
            where TResponse : class
        {
            Console.WriteLine("LOGGING MIDDLEWARE WRAPPING REQUEST");
            var nextState = continuation(request, context);
            Console.WriteLine("LOGGING MIDDLEWARE WRAPPED REQUEST");
            return new MiddlewareRequestState<TResponse>(
                ResponseAsync: nextState.ResponseAsync.ContinueWith(r =>
                {
                    Console.WriteLine("LOGGING MIDDLEWARE RESPONSE CALLBACK");
                    return r.Result;
                }),
                ResponseHeadersAsync: nextState.ResponseHeadersAsync,
                GetStatus: nextState.GetStatus,
                GetTrailers: nextState.GetTrailers
            );
        }
    }
}

