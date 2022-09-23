using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Momento.Sdk.Config.Retry;

namespace Momento.Sdk.Config.Middleware;

public record struct MiddlewareRequestState<TResponse>(
    Task<TResponse> ResponseAsync,
    Task<Metadata> ResponseHeadersAsync,
    Func<Status> GetStatus,
    Func<Metadata> GetTrailers
);

/// <summary>
/// The Middleware interface allows the Configuration to provide a higher-order function that wraps all requests.
/// This allows future support for things like client-side metrics or other diagnostics helpers.
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Called as a wrapper around each request; can be used to time the request and collect metrics etc.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="continuation"></param>
    /// <returns></returns>
    public MiddlewareRequestState<TResponse> WrapRequest<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        Func<TRequest, ClientInterceptorContext<TRequest, TResponse>, MiddlewareRequestState<TResponse>> continuation
    ) where TRequest : class where TResponse : class;
}
