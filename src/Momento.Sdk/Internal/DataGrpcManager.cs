using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Momento.Protos.CacheClient;
using static System.Reflection.Assembly;
using static Grpc.Core.Interceptors.Interceptor;

namespace Momento.Sdk.Internal;

//delegate TResponse ExecuteRequestFn<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> response) where TRequest : class where TResponse : class;
//delegate TResponse MiddlewareFn<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, ExecuteRequestFn<TRequest, TResponse> continuation) where TRequest : class where TResponse : class;
// public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
//public delegate TResponse BlockingUnaryCallContinuation<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class;

internal record struct MiddlewareRequestState<TResponse>(
    Task<TResponse> ResponseAsync,
    Task<Metadata> ResponseHeadersAsync,
    Func<Status> GetStatus,
    Func<Metadata> GetTrailers
);

internal class ChrisMiddleWare
{
    //internal static TResponse ChrisMiddleWareFn<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, ExecuteRequestFn<TRequest, TResponse> continuation) where TRequest : class where TResponse: class
    //{
    //    Console.WriteLine("CHRIS BEFORE");
    //    var response = continuation(request, context);
    //    Console.WriteLine("CHRIS AFTER");
    //    return response;
    //}

    //internal Tuple<TRequest, ClientInterceptorContext<TRequest, TResponse>> BeforeRequest<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class
    //internal (TRequest, ClientInterceptorContext<TRequest, TResponse>) BeforeRequest<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class
    //{
    //    Console.WriteLine("CHRIS MIDDLEWARE BEFORE");
    //    //return new Tuple<TRequest, ClientInterceptorContext<TRequest, TResponse>>(request, context);
    //    return (request, context);
    //}

    //internal ClientInterceptorContext<TRequest, TResponse> BeforeStream<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class
    //{
    //    Console.WriteLine("CHRIS MIDDLEWARE BEFORE STREAM");
    //    return context;
    //}

    //internal TResponse AfterResponse<TResponse>(TResponse response)
    //{
    //    Console.WriteLine("CHRIS MIDDLEWARE AFTER");
    //    return response;
    //}

    internal MiddlewareRequestState<TResponse> FancyMiddleWareFn<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        Func<TRequest, ClientInterceptorContext<TRequest, TResponse>, MiddlewareRequestState<TResponse>> continuation
        ) where TRequest : class where TResponse: class {

        Console.WriteLine("CHRIS MIDDLEWARE BEFORE");
        var nextState = continuation(request, context);
        var result = new MiddlewareRequestState<TResponse>(
            ResponseAsync: nextState.ResponseAsync.ContinueWith(resp => {
                Console.WriteLine("CHRIS MIDDLEWARE RESPONSE CONTINUEWITH");
                return resp.Result;
            }),
            ResponseHeadersAsync: nextState.ResponseHeadersAsync,
            GetStatus: nextState.GetStatus,
            GetTrailers: nextState.GetTrailers
        );
        Console.WriteLine("CHRIS MIDDLEWARE AFTER");
        return result;
    }

    //internal AsyncUnaryCall<TResponse> MiddlewareFn<TRequest, TResponse>(
    //    TRequest request,
    //    ClientInterceptorContext<TRequest, TResponse> context,
    //    Func<TRequest, ClientInterceptorContext<TRequest, TResponse>, AsyncUnaryCall<TResponse>> continuation
    //) where TRequest : class where TResponse : class
    //{        var t = Task.FromResult(42);
    //    var t2 = t.ContinueWith(x => x.Result);
    //    Console.WriteLine("CHRIS MIDDLEWARE BEFORE");
    //    var response = continuation(request, context);
    //    var call = new AsyncUnaryCall<TResponse>(
    //        responseAsync: response.ResponseAsync.ContinueWith(resp => {
    //            Console.WriteLine("CHRIS MIDDLEWARE RESPONSE CONTINUEWITH");
    //            return resp.Result;
    //            }),
    //        responseHeadersAsync: response.ResponseHeadersAsync,
    //        getStatusFunc: response.GetStatus,
    //        getTrailersFunc: response.GetTrailers,
    //        disposeAction: response.Dispose
    //    );
    //    Console.WriteLine("CHRIS MIDDLEWARE AFTER");
    //    return call;
    //}
}

internal class ChrisInterceptor : Interceptor
{
    //public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    //{
    //    //return ChrisMiddleWare.ChrisMiddleWareFn(request, context, (r, c) => continuation(r, c));
    //    var chrisMiddleWare = new ChrisMiddleWare();
    //    return chrisMiddleWare.MiddlewareFn(request, context, (r, c) => Task.FromResult(continuation(r, c))).Result;
    //    //(var chrisRequest, var chrisContext) = chrisMiddleWare.BeforeRequest(request, context);
    //    //return chrisMiddleWare.AfterResponse(continuation(chrisRequest, chrisContext));


    //    //AddCallerMetadata(ref context);
    //    //return continuation(request, context);
    //}
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        //AddCallerMetadata(ref context);
        //return continuation(request, context);
        //return ChrisMiddleWare.ChrisMiddleWareFn(request, context, (r, c) => continuation(r, c));
        var chrisMiddleWare = new ChrisMiddleWare();
        TaskCompletionSource<AsyncUnaryCall<TResponse>> theAsyncUnaryCallTaskSource = new TaskCompletionSource<AsyncUnaryCall<TResponse>>();
        Task<AsyncUnaryCall<TResponse>> theAsyncUnaryCallTask = theAsyncUnaryCallTaskSource.Task;
        var chrisMiddleWareState = chrisMiddleWare.FancyMiddleWareFn(request, context, (r, c) =>
        {
            AsyncUnaryCall<TResponse> theAsyncUnaryCall = continuation(r, c);
            theAsyncUnaryCallTaskSource.SetResult(theAsyncUnaryCall);
            return new MiddlewareRequestState<TResponse>(
                ResponseAsync: theAsyncUnaryCall.ResponseAsync,
                ResponseHeadersAsync: theAsyncUnaryCall.ResponseHeadersAsync,
                GetStatus: theAsyncUnaryCall.GetStatus,
                GetTrailers: theAsyncUnaryCall.GetTrailers
                );
        });

        return new AsyncUnaryCall<TResponse>(
            responseAsync: chrisMiddleWareState.ResponseAsync,
            responseHeadersAsync: chrisMiddleWareState.ResponseHeadersAsync,
            getStatusFunc: chrisMiddleWareState.GetStatus,
            getTrailersFunc: chrisMiddleWareState.GetTrailers,
            disposeAction: theAsyncUnaryCallTask.Result.Dispose
        );

        //return chrisMiddleWare.MiddlewareFn(request, context, (r, c) => {



        //    var result = continuation(r, c);
        //    result.



        //    return result.ResponseAsync;


        //    });
        //var 
        //(var chrisRequest, var chrisContext) = chrisMiddleWare.BeforeRequest(request, context);
        //return chrisMiddleWare.AfterResponse(continuation(chrisRequest, chrisContext));
    }
    //public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    //{
    //    //AddCallerMetadata(ref context);
    //    //return continuation(request, context);
    //    var chrisMiddleWare = new ChrisMiddleWare();
    //    (var chrisRequest, var chrisContext) = chrisMiddleWare.BeforeRequest(request, context);
    //    return chrisMiddleWare.AfterResponse(continuation(chrisRequest, chrisContext));
    //}
    //public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    //{
    //    //AddCallerMetadata(ref context);
    //    //return continuation(context);
    //    var chrisMiddleWare = new ChrisMiddleWare();
    //    var chrisContext = chrisMiddleWare.BeforeStream(context);
    //    var executed = continuation(context);
    //    return new AsyncClientStreamingCall<TRequest, TResponse>(
    //        executed.RequestStream,
    //        executed.ResponseAsync,
    //        executed.ResponseHeadersAsync,
    //        executed.GetStatus,
    //        executed.GetTrailers,
    //        executed.Dispose
    //    );
    //}
    //public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    //{
    //    //AddCallerMetadata(ref context);
    //    //return continuation(context);
    //    //var chrisMiddleWare = new ChrisMiddleWare();
    //    //(var chrisRequest, var chrisContext) = chrisMiddleWare.BeforeRequest(request, context);
    //    //return chrisMiddleWare.AfterResponse(continuation(chrisRequest, chrisContext));

    //    var chrisMiddleWare = new ChrisMiddleWare();
    //    var chrisContext = chrisMiddleWare.BeforeStream(context);
    //    var executed = continuation(context);
    //    return new AsyncDuplexStreamingCall<TRequest, TResponse>(
    //        executed.RequestStream,
    //        executed.ResponseStream,
    //        executed.ResponseHeadersAsync,
    //        executed.GetStatus,
    //        executed.GetTrailers,
    //        executed.Dispose
    //    );
    //}
}

public class DataGrpcManager : IDisposable
{
    private readonly GrpcChannel channel;
    public Scs.ScsClient Client { get; }

    private readonly string version = "dotnet:" + GetAssembly(typeof(Momento.Sdk.Responses.CacheGetResponse)).GetName().Version.ToString();
    // Some System.Environment.Version remarks to be aware of
    // https://learn.microsoft.com/en-us/dotnet/api/system.environment.version?view=netstandard-2.0#remarks
    private readonly string runtimeVersion = "dotnet:" + System.Environment.Version;

    internal DataGrpcManager(string authToken, string host)
    {
        var url = $"https://{host}";
        this.channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions() { Credentials = ChannelCredentials.SecureSsl });
        List<Header> headers = new List<Header> { new Header(name: Header.AuthorizationKey, value: authToken), new Header(name: Header.AgentKey, value: version), new Header(name: Header.RuntimeVersionKey, value: runtimeVersion) };


        



        CallInvoker invoker = this.channel
            .Intercept(new ChrisInterceptor())
            .Intercept(new HeaderInterceptor(headers))
            ;
        Client = new Scs.ScsClient(invoker);
    }

    public void Dispose()
    {
        this.channel.Dispose();
        GC.SuppressFinalize(this);
    }
}
