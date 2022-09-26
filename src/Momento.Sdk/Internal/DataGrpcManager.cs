using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Momento.Protos.CacheClient;
using Momento.Sdk.Config;
using Momento.Sdk.Config.Middleware;
using static System.Reflection.Assembly;
using static Grpc.Core.Interceptors.Interceptor;

namespace Momento.Sdk.Internal;

public interface IDataClient
{
    public Task<_GetResponse> GetAsync(_GetRequest request, CallOptions callOptions);
    public Task<_SetResponse> SetAsync(_SetRequest request, CallOptions callOptions);
    public Task<_DeleteResponse> DeleteAsync(_DeleteRequest request, CallOptions callOptions);
}

internal class DataClientWithMiddleware : IDataClient
{
    private readonly IList<IMiddleware> _middlewares;
    private readonly Scs.ScsClient _generatedClient;

    public DataClientWithMiddleware(Scs.ScsClient generatedClient, IList<IMiddleware> middlewares)
    {
        _generatedClient = generatedClient;
        _middlewares = middlewares;
    }

    public async Task<_DeleteResponse> DeleteAsync(_DeleteRequest request, CallOptions callOptions)
    {
        var wrapped = await WrapWithMiddleware(request, callOptions, (r, o) => _generatedClient.DeleteAsync(r, o));
        return await wrapped.ResponseAsync;
    }

    public async Task<_GetResponse> GetAsync(_GetRequest request, CallOptions callOptions)
    {
        var wrapped = await WrapWithMiddleware(request, callOptions, (r, o) => _generatedClient.GetAsync(r, o));
        return await wrapped.ResponseAsync;
    }

    public async Task<_SetResponse> SetAsync(_SetRequest request, CallOptions callOptions)
    {
        var wrapped = await WrapWithMiddleware(request, callOptions, (r, o) => _generatedClient.SetAsync(r, o));
        return await wrapped.ResponseAsync;
    }


    private async Task<MiddlewareResponseState<TResponse>> WrapWithMiddleware<TRequest, TResponse>(
        TRequest request,
        CallOptions callOptions,
        Func<TRequest, CallOptions, AsyncUnaryCall<TResponse>> continuation
    )
    {
        Func<TRequest, CallOptions, Task<MiddlewareResponseState<TResponse>>> continuationWithMiddlewareResponseState = (r, o) =>
        {
            var result = continuation(r, o);
            return Task.FromResult(new MiddlewareResponseState<TResponse>(
                ResponseAsync: result.ResponseAsync,
                ResponseHeadersAsync: result.ResponseHeadersAsync,
                GetStatus: result.GetStatus,
                GetTrailers: result.GetTrailers
            ));
        };

        var wrapped = _middlewares.Aggregate(continuationWithMiddlewareResponseState, (acc, middleware) =>
        {
            return (r, o) => middleware.WrapRequest(r, o, acc);
        });
        return await wrapped(request, callOptions);
    }
}

public class DataGrpcManager : IDisposable
{
    private readonly GrpcChannel channel;

    public readonly IDataClient Client;

    private readonly string version = "dotnet:" + GetAssembly(typeof(Momento.Sdk.Responses.CacheGetResponse)).GetName().Version.ToString();
    // Some System.Environment.Version remarks to be aware of
    // https://learn.microsoft.com/en-us/dotnet/api/system.environment.version?view=netstandard-2.0#remarks
    private readonly string runtimeVersion = "dotnet:" + System.Environment.Version;

    internal DataGrpcManager(IConfiguration config, string authToken, string host)
    {
        var url = $"https://{host}";
        this.channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions() { Credentials = ChannelCredentials.SecureSsl });
        List<Header> headers = new List<Header> { new Header(name: Header.AuthorizationKey, value: authToken), new Header(name: Header.AgentKey, value: version), new Header(name: Header.RuntimeVersionKey, value: runtimeVersion) };

        CallInvoker invoker = this.channel
            .Intercept(new HeaderInterceptor(headers));

        var middlewares = config.Middlewares.Concat(
            new List<IMiddleware> { new MaxConcurrentRequestsMiddleware(config.TransportStrategy.MaxConcurrentRequests) }
        ).ToList();

        Client = new DataClientWithMiddleware(new Scs.ScsClient(invoker), middlewares);
    }

    public void Dispose()
    {
        this.channel.Dispose();
        GC.SuppressFinalize(this);
    }
}