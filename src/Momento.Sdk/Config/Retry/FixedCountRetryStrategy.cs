using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Momento.Sdk.Config.Retry;

public class FixedCountRetryStrategy : IRetryStrategy
{
    public ILoggerFactory? LoggerFactory { get; }

    private ILogger _logger;

    public int MaxAttempts { get; }

    
    //FixedCountRetryStrategy(retryableStatusCodes = DEFAULT_RETRYABLE_STATUS_CODES, maxAttempts = 3),
    public FixedCountRetryStrategy(int maxAttempts, ILoggerFactory? loggerFactory = null)
    {
        LoggerFactory = loggerFactory;
        _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<FixedCountRetryStrategy>();
        MaxAttempts = maxAttempts;
    }

    public FixedCountRetryStrategy WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return new(MaxAttempts, loggerFactory);
    }

    IRetryStrategy IRetryStrategy.WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return WithLoggerFactory(loggerFactory);
    }

    public FixedCountRetryStrategy WithMaxAttempts(int maxAttempts)
    {
        return new(maxAttempts, LoggerFactory);
    }

    public int? DetermineWhenToRetryRequest<TRequest>(Status grpcStatus, TRequest grpcRequest, int attemptNumber)
    {
        _logger.LogDebug($"Determining whether request is eligible for retry; status code: {grpcStatus.StatusCode}, request type: {grpcRequest?.GetType()}, attemptNumber: {attemptNumber}, maxAttempts: {MaxAttempts}");
        if (attemptNumber > MaxAttempts)
        {
            _logger.LogDebug($"Exceeded max retry count ({MaxAttempts})");
            return null;
        }
        _logger.LogDebug($"Request is eligible for retry (attempt {attemptNumber} of {MaxAttempts}, retrying immediately.");
        return 0;
    }

}
