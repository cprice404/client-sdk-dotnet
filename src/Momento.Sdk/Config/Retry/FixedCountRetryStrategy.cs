using Microsoft.Extensions.Logging;

namespace Momento.Sdk.Config.Retry;

public class FixedCountRetryStrategy : IRetryStrategy
{
    public ILoggerFactory LoggerFactory { get; }
    public int MaxAttempts { get; }

    //FixedCountRetryStrategy(retryableStatusCodes = DEFAULT_RETRYABLE_STATUS_CODES, maxAttempts = 3),
    public FixedCountRetryStrategy(ILoggerFactory loggerFactory, int maxAttempts)
    {
        LoggerFactory = loggerFactory;
        MaxAttempts = maxAttempts;
    }

    public FixedCountRetryStrategy WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return new(loggerFactory, MaxAttempts);
    }

    IRetryStrategy IRetryStrategy.WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return WithLoggerFactory(loggerFactory);
    }

    ILoggerConsumer ILoggerConsumer.WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return WithLoggerFactory(loggerFactory);
    }

    public FixedCountRetryStrategy WithMaxAttempts(int maxAttempts)
    {
        return new(LoggerFactory, maxAttempts);
    }

    public int? DetermineWhenToRetryRequest(IGrpcResponse grpcResponse, IGrpcRequest grpcRequest, int attemptNumber)
    {
        if (attemptNumber > MaxAttempts)
        {
            return null;
        }
        return 0;
    }
}
