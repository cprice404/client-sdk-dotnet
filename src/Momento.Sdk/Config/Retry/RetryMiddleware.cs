using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Momento.Sdk.Config.Middleware;
using System.Linq;

namespace Momento.Sdk.Config.Retry
{
    public class RetryMiddleware : IMiddleware
    {

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
            _logger = loggerFactory.CreateLogger<RetryMiddleware>();
            _retryStrategy = new FixedCountRetryStrategy(loggerFactory, 3);
        }

        public async Task<MiddlewareResponseState<TResponse>> WrapRequest<TRequest, TResponse>(
            TRequest request,
            CallOptions callOptions,
            Func<TRequest, CallOptions, Task<MiddlewareResponseState<TResponse>>> continuation
        )
        {
            //_logger.LogDebug("Executing request of type: {}", request?.GetType());
            //Console.WriteLine($"MIDDLEWARE BEFORE FIRST CONTINUATION: HEADERS:");
            //PrintTheStupidMetadata(callOptions.Headers!);
            var nextState = await continuation(request, callOptions);
            try
            {
                await nextState.ResponseAsync;
            } catch (Exception)
            {
                int attemptNumber = 1;
                int? retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
                while (retryAfterMillis != null) {
                    attemptNumber++;
                    nextState = await continuation(request, callOptions);
                    try
                    {
                        await nextState.ResponseAsync;
                    } catch (Exception)
                    {
                        retryAfterMillis = _retryStrategy.DetermineWhenToRetryRequest(nextState.GetStatus(), request, attemptNumber);
                    }
                }
            }



            //Console.WriteLine($"MIDDLEWARE AFTER FIRST CONTINUATION: HEADERS:");
            //PrintTheStupidMetadata(callOptions.Headers!);
            try
            {
                await nextState.ResponseAsync;
            }
            catch (Exception ex)
            {
                var status = nextState.GetStatus();
                if (status.StatusCode == StatusCode.Unavailable)
                {
                    _logger.LogDebug("RETRY INTERCEPTOR GOT AN UNAVAILBLE, RETRYING EXACTLY ONE TIME");
                    //Console.WriteLine($"MIDDLEWARE BEFORE SECOND CONTINUATION: HEADERS:");
                    //PrintTheStupidMetadata(callOptions.Headers!);
                    nextState = await continuation(request, callOptions);
                    //Console.WriteLine($"MIDDLEWARE AFTER SECOND CONTINUATION: HEADERS:");
                    PrintTheStupidMetadata(callOptions.Headers!);
                    try
                    {
                        await nextState.ResponseAsync;
                        _logger.LogDebug("RETRY SUCCESSFUL");
                    } catch (Exception ex2)
                    {
                        _logger.LogDebug("RETRY FAILED! {}", ex2);
                    }
                }
                else
                {
                    _logger.LogDebug("ARE YOU SERIOUS {}", ex);
                    throw ex;
                }
            }

            //_logger.LogDebug($"RETRY INTERCEPTOR OBSERVED STATUS: {status}");
            return new MiddlewareResponseState<TResponse>(
                ResponseAsync: nextState.ResponseAsync,
                //.ContinueWith(r =>
                //{
                //    //_logger.LogDebug("Got response for request of type: {}", request?.GetType());
                //    //return r.Result;
                //}),
                ResponseHeadersAsync: nextState.ResponseHeadersAsync,
                GetStatus: nextState.GetStatus,
                GetTrailers: nextState.GetTrailers
            );
        }
    }

}

