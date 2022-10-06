using Microsoft.Extensions.Logging;

namespace Momento.Sdk.Config.Transport;

/// <summary>
/// This is responsible for configuring network tunables.
/// </summary>
public interface ITransportStrategy
    //: ILoggerConsumer
{
    public int MaxConcurrentRequests { get; }
    public IGrpcConfiguration GrpcConfig { get; }

    public ITransportStrategy WithLoggerFactory(ILoggerFactory loggerFactory);
    public ITransportStrategy WithMaxConcurrentRequests(int maxConcurrentRequests);
    public ITransportStrategy WithGrpcConfig(IGrpcConfiguration grpcConfig);
    public ITransportStrategy WithClientTimeoutMillis(uint clientTimeoutMillis);
}
