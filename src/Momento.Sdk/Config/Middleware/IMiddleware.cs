using System;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Momento.Sdk.Config.Retry;

namespace Momento.Sdk.Config.Middleware;

/// <summary>
/// The Middleware interface allows the Configuration to provide a higher-order function that wraps all requests.
/// This allows future support for things like client-side metrics or other diagnostics helpers.
/// </summary>
public interface IMiddleware
{
    public delegate Task<IGrpcResponse> MiddlewareFn(IGrpcRequest request);

    ///// <summary>
    ///// For unary gRPC requests, this middleware function will be called before the
    ///// request is issued.  It may return a modified request and or context.
    ///// </summary>
    ///// <typeparam name="TRequest"></typeparam>
    ///// <typeparam name="TResponse"></typeparam>
    ///// <param name="request"></param>
    ///// <param name="context"></param>
    ///// <returns></returns>
    //public (TRequest, ClientInterceptorContext<TRequest, TResponse>) BeforeRequest<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class
    //{
    //    Console.WriteLine("CHRIS MIDDLEWARE BEFORE");
    //    //return new Tuple<TRequest, ClientInterceptorContext<TRequest, TResponse>>(request, context);
    //    return (request, context);
    //}

    // TODO: this should return another delegate, ie
    // wrapRequest(middlewareFn) -> middlewareFn
    /// <summary>
    /// Called as a wrapper around each request; can be used to time the request and collect metrics etc.
    /// </summary>
    /// <param name="middlewareFn"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<IGrpcResponse> wrapRequest(MiddlewareFn middlewareFn, IGrpcRequest request);
}
