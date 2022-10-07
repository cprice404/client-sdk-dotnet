using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Momento.Sdk.Config.Middleware;
using System.Linq;
using Momento.Protos.CacheClient;

namespace Momento.Sdk.Config.Retry
{
    public class RetryMiddleware : IMiddleware
    {
        public ILoggerFactory? LoggerFactory { get; }

        private void PrintTheStupidMetadata(Metadata metadata)
        {
            metadata.ToList().Select(e =>
            {
                Console.WriteLine($"{e.Key}: {e.Value}");
                return 42;
            }).ToList();
        }

        private readonly ILogger _logger;
        private readonly IRetryStrategy _retryStrategy;


        public RetryMiddleware(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;

            _logger = loggerFactory.CreateLogger<RetryMiddleware>();

            // TODO
            // TODO get this from config
            // TODO
            _retryStrategy = new FixedCountRetryStrategy(3, LoggerFactory);
        }

        public RetryMiddleware WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            return new(loggerFactory);
        }

        IMiddleware IMiddleware.WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            return WithLoggerFactory(loggerFactory);
        }

        public async Task<MiddlewareResponseState<TResponse>> WrapRequest<TRequest, TResponse>(
            TRequest request,
            CallOptions callOptions,
            Func<TRequest, CallOptions, Task<MiddlewareResponseState<TResponse>>> continuation
        )
        {
            MiddlewareResponseState<TResponse> nextState;
            int attemptNumber = -1;
            int? retryAfterMillis = 0;
            do
            {
                var delay = retryAfterMillis ?? 0;
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
                attemptNumber++;
                nextState = await continuation(request, callOptions);
                try
                {
                    await nextState.ResponseAsync;
                    // TODO
                    // TODO
                    // TODO
                    if (attemptNumber > 0)
                    {
                        _logger.LogDebug($"Retry succeeded (attempt {attemptNumber})");
                    }
                    break;
                }
                catch (Exception)
                {
                    var status = nextState.GetStatus();
                    _logger.LogDebug($"Request failed with status {status.StatusCode}, checking to see if we should retry; attempt Number: {attemptNumber}");
                    _logger.LogTrace($"Failed request status: {status}");
                    retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
                }
            }
            while (retryAfterMillis != null);

            return new MiddlewareResponseState<TResponse>(
                ResponseAsync: nextState.ResponseAsync,
                ResponseHeadersAsync: nextState.ResponseHeadersAsync,
                GetStatus: nextState.GetStatus,
                GetTrailers: nextState.GetTrailers
            );

            //var nextState = await continuation(request, callOptions);
            //try
            //{
            //    await nextState.ResponseAsync;
            //}
            //catch (Exception)
            //{
            //    int attemptNumber = 1;
            //    int? retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
            //    while (retryAfterMillis != null)
            //    {
            //        String cacheKey = "NO IDEA";
            //        if (request is _GetRequest getRequest)
            //        {
            //            cacheKey = getRequest.CacheKey.ToStringUtf8();
            //        }
            //        else if (request is _SetRequest setRequest)
            //        {
            //            cacheKey = setRequest.CacheKey.ToStringUtf8();
            //        }
            //        _logger.LogDebug($"Incrementing attempt number for {cacheKey}: {attemptNumber}");
            //        attemptNumber++;
            //        nextState = await continuation(request, callOptions);
            //        try
            //        {
            //            await nextState.ResponseAsync;
            //            _logger.LogDebug("Retry succeeded!");
            //            break;
            //        }
            //        catch (Exception)
            //        {
            //            _logger.LogDebug($"Retry failed, checking to see if we should retry again for {cacheKey}: {attemptNumber}");
            //            retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
            //        }
            //    }
            //    //var nextState = await continuation(request, callOptions);
            //    //try
            //    //{
            //    //    await nextState.ResponseAsync;
            //    //} catch (Exception)
            //    //{
            //    //    int attemptNumber = 1;
            //    //    int? retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
            //    //    while (retryAfterMillis != null) {
            //    //        String cacheKey = "NO IDEA";
            //    //        if (request is _GetRequest getRequest)
            //    //        {
            //    //            cacheKey = getRequest.CacheKey.ToStringUtf8();
            //    //        } else if (request is _SetRequest setRequest)
            //    //        {
            //    //            cacheKey = setRequest.CacheKey.ToStringUtf8();
            //    //        }
            //    //        _logger.LogDebug($"Incrementing attempt number for {cacheKey}: {attemptNumber}");
            //    //        attemptNumber++;
            //    //        nextState = await continuation(request, callOptions);
            //    //        try
            //    //        {
            //    //            await nextState.ResponseAsync;
            //    //            _logger.LogDebug("Retry succeeded!");
            //    //            break;
            //    //        } catch (Exception)
            //    //        {
            //    //            _logger.LogDebug($"Retry failed, checking to see if we should retry again for {cacheKey}: {attemptNumber}");
            //    //            retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
            //    //        }
            //    //    }
            //}

        }
    }
}

