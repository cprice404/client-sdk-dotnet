using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Momento.Sdk.Config.Middleware;
using Momento.Sdk.Config.Retry;
using Momento.Sdk.Config.Transport;

namespace Momento.Sdk.Config;


/// <summary>
/// Contract for SDK configurables. A configuration must have a retry strategy, middlewares, and transport strategy.
/// </summary>
public interface IConfiguration : ILoggerConsumer
{
    public IRetryStrategy RetryStrategy { get; }
    public IList<IMiddleware> Middlewares { get; }
    public ITransportStrategy TransportStrategy { get; }

    public new IConfiguration WithLoggerFactory(ILoggerFactory loggerFactory);
    public IConfiguration WithRetryStrategy(IRetryStrategy retryStrategy);
    public IConfiguration WithMiddlewares(IList<IMiddleware> middlewares);
    public IConfiguration WithTransportStrategy(ITransportStrategy transportStrategy);
}
