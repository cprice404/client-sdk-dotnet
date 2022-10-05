using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Momento.Sdk.Config.Retry;

public class FixedCountRetryStrategy : IRetryStrategy
{
    private ILoggerFactory _loggerFactory;
    private ILogger _logger;

    public int MaxAttempts { get; }
    //FixedCountRetryStrategy(retryableStatusCodes = DEFAULT_RETRYABLE_STATUS_CODES, maxAttempts = 3),
    public FixedCountRetryStrategy(ILoggerFactory loggerFactory, int maxAttempts)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<FixedCountRetryStrategy>();
        MaxAttempts = maxAttempts;
    }

    public FixedCountRetryStrategy WithMaxAttempts(int maxAttempts)
    {
        return new(_loggerFactory, maxAttempts);
    }

    public int? DetermineWhenToRetryRequest<TRequest>(Status grpcStatus, TRequest grpcRequest, int attemptNumber)
    {
        _logger.LogDebug($"Determining whether request is eligible for retry; attemptNumber: {attemptNumber}, maxAttempts: {MaxAttempts}");
        if (attemptNumber > MaxAttempts)
        {
            _logger.LogDebug($"Exceeded max retry count ({MaxAttempts})");
            return null;
        }
        _logger.LogDebug($"Request is eligible for retry (attempt {attemptNumber} of {MaxAttempts}, retrying immediately.");
        return 0;
    }
}
