using System;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Momento.Sdk.Config.Transport;


public class StaticGrpcConfiguration : IGrpcConfiguration
{
    public uint DeadlineMilliseconds { get; }
    public GrpcChannelOptions GrpcChannelOptions { get; }

    public StaticGrpcConfiguration(uint deadlineMilliseconds, GrpcChannelOptions? grpcChannelOptions = null)
    {
        if (deadlineMilliseconds <= 0)
        {
            throw new ArgumentException($"Deadline must be strictly positive. Value was: {deadlineMilliseconds}", "DeadlineMilliseconds");
        }
        this.DeadlineMilliseconds = deadlineMilliseconds;
        this.GrpcChannelOptions = grpcChannelOptions ?? new GrpcChannelOptions();
    }

    public IGrpcConfiguration WithDeadlineMilliseconds(uint deadlineMilliseconds)
    {
        return new StaticGrpcConfiguration(deadlineMilliseconds, this.GrpcChannelOptions);
    }

    public IGrpcConfiguration WithGrpcChannelOptions(GrpcChannelOptions grpcChannelOptions)
    {
        return new StaticGrpcConfiguration(this.DeadlineMilliseconds, grpcChannelOptions);
    }
}

public class StaticTransportStrategy : ITransportStrategy
{
    public ILoggerFactory LoggerFactory { get; }
    public int MaxConcurrentRequests { get; }
    public IGrpcConfiguration GrpcConfig { get; }

    public StaticTransportStrategy(ILoggerFactory loggerFactory, int maxConcurrentRequests, IGrpcConfiguration grpcConfig)
    {
        LoggerFactory = loggerFactory;
        MaxConcurrentRequests = maxConcurrentRequests;
        GrpcConfig = grpcConfig;
    }

    public StaticTransportStrategy WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return new(loggerFactory, MaxConcurrentRequests, GrpcConfig);
    }

    ITransportStrategy ITransportStrategy.WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return WithLoggerFactory(loggerFactory);
    }

    ILoggerConsumer ILoggerConsumer.WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        return WithLoggerFactory(loggerFactory);
    }

    public ITransportStrategy WithMaxConcurrentRequests(int maxConcurrentRequests)
    {
        return new StaticTransportStrategy(LoggerFactory, maxConcurrentRequests, GrpcConfig);
    }

    public ITransportStrategy WithGrpcConfig(IGrpcConfiguration grpcConfig)
    {
        return new StaticTransportStrategy(LoggerFactory, MaxConcurrentRequests, grpcConfig);
    }

    public ITransportStrategy WithClientTimeoutMillis(uint clientTimeoutMillis)
    {
        return new StaticTransportStrategy(LoggerFactory, MaxConcurrentRequests, GrpcConfig.WithDeadlineMilliseconds(clientTimeoutMillis));
    }

}
