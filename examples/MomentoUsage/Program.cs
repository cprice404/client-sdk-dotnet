using System;
using Microsoft.Extensions.Logging;
using Momento.Sdk;
using Momento.Sdk.Auth;
using Momento.Sdk.Config;
using Momento.Sdk.Config.Transport;
using Momento.Sdk.Responses;

ICredentialProvider authProvider = new EnvMomentoTokenProvider("MOMENTO_AUTH_TOKEN");
const string CACHE_NAME = "cache";
// const string KEY = "MyKey";
// const string VALUE = "MyData";
// TimeSpan DEFAULT_TTL = TimeSpan.FromSeconds(60);


ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "hh:mm:ss ";
    });
    builder.AddFilter("Grpc.Net.Client", LogLevel.Error);
    // builder.SetMinimumLevel(LogLevel.Trace);
    builder.SetMinimumLevel(LogLevel.Information);
});

// using (ICacheClient client = new CacheClient(Configurations.Laptop.V1(), authProvider, DEFAULT_TTL))
// {
//     var createCacheResponse = await client.CreateCacheAsync(CACHE_NAME);
//     if (createCacheResponse is CreateCacheResponse.Error createError)
//     {
//         Console.WriteLine($"Error creating cache: {createError.Message}. Exiting.");
//         Environment.Exit(1);
//     }
//
//     Console.WriteLine($"Setting key: {KEY} with value: {VALUE}");
//     var setResponse = await client.SetAsync(CACHE_NAME, KEY, VALUE);
//     if (setResponse is CacheSetResponse.Error setError)
//     {
//         Console.WriteLine($"Error setting value: {setError.Message}. Exiting.");
//         Environment.Exit(1);
//     }
//
//     Console.WriteLine($"Get value for key: {KEY}");
//     CacheGetResponse getResponse = await client.GetAsync(CACHE_NAME, KEY);
//     if (getResponse is CacheGetResponse.Hit hitResponse)
//     {
//         Console.WriteLine($"Looked up value: {hitResponse.ValueString}, Stored value: {VALUE}");
//     }
//     else if (getResponse is CacheGetResponse.Error getError)
//     {
//         Console.WriteLine($"Error getting value: {getError.Message}");
//     }
// }


// var loggerFactory = LoggerFactory.Create(builder =>
// {
//     builder.AddSimpleConsole(options =>
//     {
//         options.IncludeScopes = true;
//         options.SingleLine = true;
//         options.TimestampFormat = "hh:mm:ss ";
//     });
//     builder.AddFilter("Grpc.Net.Client", LogLevel.Error);
//     builder.SetMinimumLevel(LogLevel.Information);
// });
ITransportStrategy transportStrategy = new StaticTransportStrategy(
    loggerFactory: loggerFactory,
    maxConcurrentRequests: 200,
    grpcConfig: new StaticGrpcConfiguration(deadline: TimeSpan.FromMilliseconds(15000))
);
TopicConfiguration topicConfiguration = new TopicConfiguration(loggerFactory, transportStrategy);
using (TopicClient topicClient = new TopicClient(topicConfiguration, new EnvMomentoTokenProvider("MOMENTO_AUTH_TOKEN")))
{
    Console.WriteLine("WE GOT A CLIENT");

    var numTopics = 20;
    
    var subscriptionResponses = await Task.WhenAll(
        Enumerable.Range(1, numTopics).Select(i =>
            topicClient.SubscribeAsync(CACHE_NAME, $"topic{i}")
                .ContinueWith(r => Tuple.Create(i, r.Result))));

    var subscriptions = subscriptionResponses.Select(t =>
    {
        var (topicNum, subscriptionResponse) = t;
        if (subscriptionResponse is TopicSubscribeResponse.Subscription subscription)
        {
            return Tuple.Create(topicNum, subscription);
        }

        throw new Exception($"Got an unexpected subscription response: {subscriptionResponse}");
    });
    
    Console.WriteLine("All subscriptions created!");

    var subscribers = subscriptions.Select(t => Task.Run(async () =>
    {
        var (topicNum, subscription) = t;
        await foreach (var message in subscription)
        {
            Console.WriteLine($"Received message on topic {topicNum}: '{message}'");
        }
    }));
    
    Console.WriteLine("Created subscriber tasks");

    await Task.Delay(2_000);

    foreach (var i in Enumerable.Range(0, 100))
    {
        var randomTopic = Random.Shared.NextInt64(numTopics) + 1;
        var messageId = $"message{i}";
        var topic = $"topic{randomTopic}";
        Console.WriteLine($"Publishing {messageId} to {topic}");
        var publishResponse = await topicClient.PublishAsync(CACHE_NAME, topic, messageId);
        Console.WriteLine($"Publish response: {publishResponse}");
    }
    
    Console.WriteLine("Finished publishing");

    await Task.Delay(1_000);
    
    Console.WriteLine("Cancelling subscriptions!");
    
    // foreach (var subscriptionTuple in subscriptions)
    // {
    //     var (topicNum, subscription) = subscriptionTuple;
    //     subscription.Dispose();
    // }
    
    Console.WriteLine("Awaiting subscribers");
    await Task.WhenAll(subscribers);
    
    Console.WriteLine("Subscribers completed");
}