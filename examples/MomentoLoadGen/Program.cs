using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using HdrHistogram;
using Microsoft.Extensions.Logging;
using Momento.Sdk;
using Momento.Sdk.Exceptions;

namespace MomentoLoadGen // Note: actual namespace depends on the project name.
{

        //import
        //{
        //    AlreadyExistsError,
        //  CacheGetStatus,
        //  getLogger,
        //  InternalServerError,
        //  LimitExceededError,
        //  LogFormat,
        //  Logger,
        //  LoggerOptions,
        //  LogLevel,
        //  initializeMomentoLogging,
        //  SimpleCacheClient,
        //  TimeoutError,
        //}
        //from '@gomomento/sdk';
        //import * as hdr from 'hdr-histogram-js';
        //import
        //{ range}
        //from './utils/collections';
        //import
        //{ delay}
        //from './utils/time';

        //interface BasicJavaScriptLoadGenOptions
        //{
        //    loggerOptions: LoggerOptions;
        //  requestTimeoutMs: number;
        //  cacheItemPayloadBytes: number;
        //  numberOfConcurrentRequests: number;
        //  totalNumberOfOperationsToExecute: number;
        //}

    public record CsharpLoadGeneratorOptions
    (
        uint requestTimeoutMs,
        int cacheItemPayloadBytes,
        int numberOfConcurrentRequests,
        int totalNumberOfOperationsToExecute
    );

    enum AsyncSetGetResult
    {
        SUCCESS,
        UNAVAILABLE,
        DEADLINE_EXCEEDED,
        RESOURCE_EXHAUSTED,
        RST_STREAM
    };

    //enum AsyncSetGetResult
    //{
    //    SUCCESS = 'SUCCESS',
    //    UNAVAILABLE = 'UNAVAILABLE',
    //    DEADLINE_EXCEEDED = 'DEADLINE_EXCEEDED',
    //    RESOURCE_EXHAUSTED = 'RESOURCE_EXHAUSTED',
    //    RST_STREAM = 'RST_STREAM',
    //}

    //interface BasicJavasScriptLoadGenContext
    //{
    //    startTime: [number, number];
    //  getLatencies: hdr.Histogram;
    //  setLatencies: hdr.Histogram;
    //  // TODO: these could be generalized into a map structure that
    //  //  would make it possible to deal with a broader range of
    //  //  failure types more succinctly.
    //  globalRequestCount: number;
    //  globalSuccessCount: number;
    //  globalUnavailableCount: number;
    //  globalDeadlineExceededCount: number;
    //  globalResourceExhaustedCount: number;
    //  globalRstStreamCount: number;
    //}

    internal class CsharpLoadGeneratorContext
    {
        public Stopwatch StartTime;
        public Recorder GetLatencies;
        public Recorder SetLatencies;

        public int GlobalRequestCount;
        public int GlobalSuccessCount;
        public int GlobalUnavailableCount;
        public int GlobalDeadlineExceededCount;
        public int GlobalResourceExhaustedCount;
        public int GlobalRstStreamCount;

        public CsharpLoadGeneratorContext()
        {
            StartTime = System.Diagnostics.Stopwatch.StartNew();
            GetLatencies = HistogramFactory.With64BitBucketSize().WithValuesFrom(1).WithValuesUpTo(TimeStamp.Minutes(1)).WithPrecisionOf(1).WithThreadSafeWrites().WithThreadSafeReads().Create();
            SetLatencies = HistogramFactory.With64BitBucketSize().WithValuesFrom(1).WithValuesUpTo(TimeStamp.Minutes(1)).WithPrecisionOf(1).WithThreadSafeWrites().WithThreadSafeReads().Create();

            GlobalRequestCount = 0;
            GlobalSuccessCount = 0;
            GlobalDeadlineExceededCount = 0;
            GlobalResourceExhaustedCount = 0;
            GlobalRstStreamCount = 0;
            GlobalUnavailableCount = 0;
        }
    };


    public class CsharpLoadGenerator
    {
        const int CACHE_ITEM_TTL_SECONDS = 60;
        const string CACHE_NAME = "momento-loadgen";
        const int PRINT_STATS_EVERY_N_REQUESTS = 1000;

        private readonly ILogger<CsharpLoadGenerator> _logger;
        private readonly CsharpLoadGeneratorOptions _options;
        private readonly string _cacheValue;

        public CsharpLoadGenerator(ILoggerFactory loggerFactory, CsharpLoadGeneratorOptions options)
        {
            _logger = loggerFactory.CreateLogger<CsharpLoadGenerator>();

            _options = options;

            _cacheValue = new String('x', _options.cacheItemPayloadBytes);
        }


        //class BasicJavaScriptLoadGen
        //{
        //    private readonly logger: Logger;
        //  private readonly cacheItemTtlSeconds = 60;
        //  private readonly authToken: string;
        //  private readonly loggerOptions: LoggerOptions;
        //  private readonly requestTimeoutMs: number;
        //  private readonly numberOfConcurrentRequests: number;
        //  private readonly totalNumberOfOperationsToExecute: number;
        //  private readonly cacheValue: string;

        //  private readonly cacheName: string = 'js-loadgen';

        //  constructor(options: BasicJavaScriptLoadGenOptions)
        //    {
        //        initializeMomentoLogging(options.loggerOptions);
        //        this.logger = getLogger('load-gen');
        //        const authToken = process.env.MOMENTO_AUTH_TOKEN;
        //        if (!authToken)
        //        {
        //            throw new Error(
        //              'Missing required environment variable MOMENTO_AUTH_TOKEN'
        //            );
        //        }
        //        this.loggerOptions = options.loggerOptions;
        //        this.authToken = authToken;
        //        this.requestTimeoutMs = options.requestTimeoutMs;
        //        this.numberOfConcurrentRequests = options.numberOfConcurrentRequests;
        //        this.totalNumberOfOperationsToExecute =
        //          options.totalNumberOfOperationsToExecute;

        //        this.cacheValue = 'x'.repeat(options.cacheItemPayloadBytes);
        //    }

        //    async run(): Promise<void> {
        //    const momento = new SimpleCacheClient(
        //      this.authToken,
        //      this.cacheItemTtlSeconds,
        //      {
        //        requestTimeoutMs: this.requestTimeoutMs,
        //        loggerOptions: this.loggerOptions,
        //      }
        //    );

    public async Task Run()
        {
            string? authToken = System.Environment.GetEnvironmentVariable("MOMENTO_AUTH_TOKEN");
            if (authToken == null)
            {
                throw new Exception("Missing required environment variable MOMENTO_AUTH_TOKEN");
            }

            var momento = new SimpleCacheClient(
                authToken,
                CACHE_ITEM_TTL_SECONDS,
                _options.requestTimeoutMs
            );

            try
            {
                momento.CreateCache(CACHE_NAME);
            } catch (AlreadyExistsException)
            {
                _logger.LogInformation("cache '{0}' already exists", CACHE_NAME);
            }
            

            var numOperationsPerWorker = _options.totalNumberOfOperationsToExecute / _options.numberOfConcurrentRequests;

            var context = new CsharpLoadGeneratorContext();


            var asyncResults = Enumerable.Range(0, _options.numberOfConcurrentRequests).Select<int, Task<int>>(workerId =>
                LaunchAndRunWorkers(
                    momento,
                    context,
                    workerId + 1,
                    numOperationsPerWorker
                    )
            );

            var allResults = await Task.WhenAll(asyncResults);
            _logger.LogInformation("Done");
        }

        //const loadGenContext: BasicJavasScriptLoadGenContext = {
        //startTime: process.hrtime(),
        //      getLatencies: hdr.build(),
        //      setLatencies: hdr.build(),
        //      globalRequestCount: 0,
        //      globalSuccessCount: 0,
        //      globalUnavailableCount: 0,
        //      globalDeadlineExceededCount: 0,
        //      globalResourceExhaustedCount: 0,
        //      globalRstStreamCount: 0,
        //    };


        //  private async launchAndRunWorkers(
        //    client: SimpleCacheClient,
        //    loadGenContext: BasicJavasScriptLoadGenContext,
        //    workerId: number,
        //    numOperations: number
        //  ): Promise<void> {
        //    for (let i = 1; i <= numOperations; i++)
        //    {
        //        await this.issueAsyncSetGet(client, loadGenContext, workerId, i);

        //        if (loadGenContext.globalRequestCount % 1000 === 0)
        //        {
        //            this.logger.info(`
        //cumulative stats:
        //       total requests: ${
        //                loadGenContext.globalRequestCount
        //       } (${
        //                BasicJavaScriptLoadGen.tps(
        //          loadGenContext,
        //          loadGenContext.globalRequestCount
        //        )}
        //            tps)
        //           success: ${
        //        loadGenContext.globalSuccessCount
        //           } (${
        //        BasicJavaScriptLoadGen.percentRequests(
        //          loadGenContext,
        //          loadGenContext.globalSuccessCount
        //        )}%) (${
        //        BasicJavaScriptLoadGen.tps(
        //          loadGenContext,
        //          loadGenContext.globalSuccessCount
        //        )}
        //    tps)
        //       unavailable: ${
        //        loadGenContext.globalUnavailableCount
        //       } (${
        //        BasicJavaScriptLoadGen.percentRequests(
        //          loadGenContext,
        //          loadGenContext.globalUnavailableCount
        //        )}%)
        // deadline exceeded: ${
        //        loadGenContext.globalDeadlineExceededCount
        // } (${
        //        BasicJavaScriptLoadGen.percentRequests(
        //          loadGenContext,
        //          loadGenContext.globalDeadlineExceededCount
        //        )}%)
        //resource exhausted: ${
        //        loadGenContext.globalResourceExhaustedCount
        //        } (${
        //        BasicJavaScriptLoadGen.percentRequests(
        //          loadGenContext,
        //          loadGenContext.globalResourceExhaustedCount
        //        )}%)
        //        rst stream: ${
        //        loadGenContext.globalRstStreamCount
        //        } (${
        //        BasicJavaScriptLoadGen.percentRequests(
        //          loadGenContext,
        //          loadGenContext.globalRstStreamCount
        //        )}%)

        //cumulative set latencies:
        //${ BasicJavaScriptLoadGen.outputHistogramSummary(loadGenContext.setLatencies)}

        //    cumulative get latencies:
        //${ BasicJavaScriptLoadGen.outputHistogramSummary(loadGenContext.getLatencies)}
        //`);
        //}
        //    }
        //  }

        private async Task<int> LaunchAndRunWorkers(
            SimpleCacheClient client,
            CsharpLoadGeneratorContext context,
            int workerId,
            int numOperations)
        {
            for (var i = 1; i < numOperations; i++)
            {
                await IssueAsyncSetGet(client, context, workerId, i);

                //_logger.LogInformation("ABOUT TO CHECK IF WE NEED TO PRINT STATS");
                if (context.GlobalRequestCount % PRINT_STATS_EVERY_N_REQUESTS == 0) {
                    //_logger.LogInformation("WE NEED TO PRINT STATS");
                    var setsHistogram = context.SetLatencies.GetIntervalHistogram();
                    Console.WriteLine($"Sets histogram count: {setsHistogram.TotalCount}, bucket count: {setsHistogram.BucketCount}");
                    var getsHistogram = context.GetLatencies.GetIntervalHistogram();
                    Console.WriteLine($"Gets histogram count: {getsHistogram.TotalCount}, bucket count: {getsHistogram.BucketCount}");
                    Console.Out.Flush();
                    try
                    {
                        Console.WriteLine("Getting p50");
                        Console.Out.Flush();
                        Console.WriteLine($"{getsHistogram.GetValueAtPercentile(50)}");
                        Console.WriteLine("Getting p90");
                        Console.Out.Flush();
                        Console.WriteLine($"{getsHistogram.GetValueAtPercentile(90)}");
                        Console.WriteLine("Getting p99");
                        Console.Out.Flush();
                        Console.WriteLine($"{getsHistogram.GetValueAtPercentile(99)}");
                        Console.WriteLine("Getting p99.9");
                        Console.Out.Flush();
                        Console.WriteLine($"{getsHistogram.GetValueAtPercentile(99.9)}");
                    } catch (ArgumentOutOfRangeException ex)
                    {
                        Console.WriteLine("\n\nCAUGHT THE WEIRD HISTOGRAM EXCEPTION!!!!\n\n");
                    }
                    //                    Console.Out.Flush();

                    //                    Console.WriteLine($@"
                    //cumulative stats:
                    //        total requests: {context.GlobalRequestCount} ({Tps(context, context.GlobalRequestCount)} tps)
                    //               success: {context.GlobalSuccessCount} ({PercentRequests(context, context.GlobalSuccessCount)}%) ({Tps(context,context.GlobalSuccessCount)} tps)
                    //           unavailable: {context.GlobalUnavailableCount} ({PercentRequests(context, context.GlobalUnavailableCount)}%)
                    //     deadline exceeded: {context.GlobalDeadlineExceededCount} ({PercentRequests(context, context.GlobalDeadlineExceededCount)}%)
                    //    resource exhausted: {context.GlobalResourceExhaustedCount} ({PercentRequests(context, context.GlobalResourceExhaustedCount)}%)
                    //            rst stream: {context.GlobalRstStreamCount} ({PercentRequests(context, context.GlobalRstStreamCount)}%)

                    //cumulative set latencies:
                    //{ OutputHistogramSummary(setsHistogram)}

                    //    cumulative get latencies:
                    //{ OutputHistogramSummary(getsHistogram)}

                    //");
                    //_logger.LogInformation("PRINTED STATS");
                };
                
            }
            return numOperations;
        }


        //  private async issueAsyncSetGet(
        //    client: SimpleCacheClient,
        //    loadGenContext: BasicJavasScriptLoadGenContext,
        //    workerId: number,
        //    operationId: number
        //  ): Promise<void> {
        //    const cacheKey = `worker${ workerId}
        //    operation${ operationId}`;

        //    const setStartTime = process.hrtime();
        //    const result = await this.executeRequestAndUpdateContextCounts(
        //      loadGenContext,
        //      () => client.set(this.cacheName, cacheKey, this.cacheValue)
        //    );
        //    if (result !== undefined)
        //    {
        //        const setDuration = BasicJavaScriptLoadGen.getElapsedMillis(setStartTime);
        //        loadGenContext.setLatencies.recordValue(setDuration);
        //    }

        //    const getStartTime = process.hrtime();
        //    const getResult = await this.executeRequestAndUpdateContextCounts(
        //      loadGenContext,
        //      () => client.get(this.cacheName, cacheKey)
        //    );

        //    if (getResult !== undefined)
        //    {
        //        const getDuration = BasicJavaScriptLoadGen.getElapsedMillis(getStartTime);
        //        loadGenContext.getLatencies.recordValue(getDuration);
        //        let valueString: string;
        //        if (getResult.status === CacheGetStatus.Hit)
        //        {
        //            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        //            const value: string = getResult.text()!;
        //            valueString = `${ value.substring(0, 10)}
        //            ... (len: ${ value.length})`;
        //        }
        //        else
        //        {
        //            valueString = 'n/a';
        //        }
        //        if (loadGenContext.globalRequestCount % 1000 === 0)
        //        {
        //            this.logger.info(
        //          `worker: ${ workerId}, worker request: ${ operationId}, global request: ${ loadGenContext.globalRequestCount}, status: ${ getResult.status}, val: ${ valueString}`
        //        );
        //        }
        //    }
        //}

        private async Task<int> IssueAsyncSetGet(SimpleCacheClient client, CsharpLoadGeneratorContext context, int workerId, int operationId)
        {
            var cacheKey = $"worker{workerId}operation{operationId}";
            var setStartTime = System.Diagnostics.Stopwatch.StartNew();
            var result = await ExecuteRequestAndUpdateContextCounts(
                context,
                () => client.SetAsync(CACHE_NAME, cacheKey, _cacheValue)
                );
            if (result != null)
            {
                var setDuration = setStartTime.ElapsedMilliseconds;
                context.SetLatencies.RecordValue(setDuration);            
            }

            var getStartTime = System.Diagnostics.Stopwatch.StartNew();
            var getResult = await ExecuteRequestAndUpdateContextCounts(
                context,
                () => client.GetAsync(CACHE_NAME, cacheKey)
                );

            if (getResult != null)
            {
                var getDuration = getStartTime.ElapsedMilliseconds;
                context.GetLatencies.RecordValue(getDuration);

                string valueString;

                if (getResult.Status == Momento.Sdk.Responses.CacheGetStatus.HIT)
                {
                    string value = getResult.String()!;
                    valueString = $"{value.Substring(0, 10)}... (len: {value.Length})";
                }
                else
                {
                    valueString = "n/a";
                }

                if (context.GlobalRequestCount % 1000 == 0)
                {
                    _logger.LogInformation($"worker: {workerId}, worker request: {operationId}, global request: {context.GlobalRequestCount}, status: ${getResult.Status}, val: ${valueString}");
                }
            }

            return 1;
        }


        //private async executeRequestAndUpdateContextCounts<T>(
        //  context: BasicJavasScriptLoadGenContext,
        //    block: () => Promise<T>
        //  ): Promise < T | undefined > {
        //    const [result, response] = await this.executeRequest(block);
        //    this.updateContextCountsForRequest(context, result);
        //    return response;
        //}

        private async Task<TResult> ExecuteRequestAndUpdateContextCounts<TResult>(
            CsharpLoadGeneratorContext context,
            Func<Task<TResult>> block
            )
        {
            var result = await ExecuteRequest(block);
            UpdateContextCountsForRequest(context, result.Item1);
            return result.Item2;
        }

        //private async executeRequest<T>(
        //  block: () => Promise<T>
        //  ): Promise <[AsyncSetGetResult, T | undefined] > {
        //    try
        //    {
        //        const result = await block();
        //        return [AsyncSetGetResult.SUCCESS, result];
        //    }
        //    catch (e)
        //    {
        //        if (e instanceof InternalServerError) {
        //            if (e.message.includes('UNAVAILABLE'))
        //            {
        //                return [AsyncSetGetResult.UNAVAILABLE, undefined];
        //            }
        //            else if (e.message.includes('RST_STREAM'))
        //            {
        //                this.logger.error(
        //            `Caught RST_STREAM error; swallowing: ${ e.name}, ${ e.message}`
        //          );
        //                return [AsyncSetGetResult.RST_STREAM, undefined];
        //            }
        //            else
        //            {
        //                throw e;
        //            }
        //        } else if (e instanceof LimitExceededError) {
        //            if (e.message.includes('RESOURCE_EXHAUSTED'))
        //            {
        //                this.logger.error(
        //            `Caught RESOURCE_EXHAUSTED error; swallowing: ${ e.name}, ${ e.message}`
        //          );
        //                return [AsyncSetGetResult.RESOURCE_EXHAUSTED, undefined];
        //            }
        //            else
        //            {
        //                throw e;
        //            }
        //        } else if (e instanceof TimeoutError) {
        //            if (e.message.includes('DEADLINE_EXCEEDED'))
        //            {
        //                return [AsyncSetGetResult.DEADLINE_EXCEEDED, undefined];
        //            }
        //            else
        //            {
        //                throw e;
        //            }
        //        } else
        //        {
        //            throw e;
        //        }
        //    }
        //}

        private async Task<Tuple<AsyncSetGetResult, T?>> ExecuteRequest<T>(
            Func<Task<T>> block
            )
        {
            try
            {
                T result = await block();
                return Tuple.Create(AsyncSetGetResult.SUCCESS, (T?) result);
            }
            catch (InternalServerException e)
            {
                var innerException = e.InnerException as RpcException;
                _logger.LogInformation("CAUGHT AN EXCEPTION WHILE EXECUTING REQUEST: {0}", innerException);
                switch (innerException!.StatusCode)
                {

                    case StatusCode.Unavailable:
                        return Tuple.Create(AsyncSetGetResult.UNAVAILABLE, default(T));
                    default:
                        throw e;
                }
            } catch (Exception e) {
                _logger.LogInformation("CAUGHT AN EXCEPTION WHILE EXECUTING REQUEST: {0}", e);
                throw e;
            }
        }

        //private updateContextCountsForRequest(
        //  context: BasicJavasScriptLoadGenContext,
        //    result: AsyncSetGetResult
        //  ): void
        //{
        //    context.globalRequestCount++;
        //    // TODO: this could be simplified and made more generic, worth doing if we ever want to
        //    //  expand this to additional types of behavior
        //    switch (result)
        //    {
        //        case AsyncSetGetResult.SUCCESS:
        //            context.globalSuccessCount++;
        //            break;
        //        case AsyncSetGetResult.UNAVAILABLE:
        //            context.globalUnavailableCount++;
        //            break;
        //        case AsyncSetGetResult.DEADLINE_EXCEEDED:
        //            context.globalDeadlineExceededCount++;
        //            break;
        //        case AsyncSetGetResult.RESOURCE_EXHAUSTED:
        //            context.globalResourceExhaustedCount++;
        //            break;
        //        case AsyncSetGetResult.RST_STREAM:
        //            context.globalRstStreamCount++;
        //            break;
        //    }
        //}

        private static void UpdateContextCountsForRequest(
            CsharpLoadGeneratorContext context,
            AsyncSetGetResult result
            )
        {
            Interlocked.Increment(ref context.GlobalRequestCount);
            var updated = result switch
            {
                AsyncSetGetResult.SUCCESS => Interlocked.Increment(ref context.GlobalSuccessCount),
                AsyncSetGetResult.UNAVAILABLE => Interlocked.Increment(ref context.GlobalUnavailableCount),
                AsyncSetGetResult.DEADLINE_EXCEEDED => Interlocked.Increment(ref context.GlobalDeadlineExceededCount),
                AsyncSetGetResult.RESOURCE_EXHAUSTED => Interlocked.Increment(ref context.GlobalResourceExhaustedCount),
                AsyncSetGetResult.RST_STREAM => Interlocked.Increment(ref context.GlobalRstStreamCount),
                _ => throw new Exception($"Unrecognized result: {result}"),
            };
            return;
        }


        //private static tps(
        //  context: BasicJavasScriptLoadGenContext,
        //    requestCount: number
        //  ): number
        //{
        //    return Math.round(
        //      (requestCount * 1000) /
        //        BasicJavaScriptLoadGen.getElapsedMillis(context.startTime)
        //    );
        //}

        private static double Tps(CsharpLoadGeneratorContext context, int requestCount)
        {
            return Math.Round((requestCount * 1000.0) / context.StartTime.ElapsedMilliseconds);
        }

        //private static percentRequests(
        //  context: BasicJavasScriptLoadGenContext,
        //    count: number
        //  ): string
        //{
        //    return (
        //      Math.round((count / context.globalRequestCount) * 100 * 10) / 10
        //    ).toString();
        //}

        private static string PercentRequests(CsharpLoadGeneratorContext context, int count)
        {
            if (context.GlobalRequestCount == 0)
            {
                return "0";
            }
            return Math.Round((count / context.GlobalRequestCount) * 100.0, 1).ToString();
            //return "FOO";
        }

        //private static outputHistogramSummary(histogram: hdr.Histogram): string
        //{
        //    return `
        //  count: ${ histogram.totalCount}
        //min: ${ histogram.minNonZeroValue}
        //p50: ${ histogram.getValueAtPercentile(50)}
        //p90: ${ histogram.getValueAtPercentile(90)}
        //p99: ${ histogram.getValueAtPercentile(99)}
        //    p99.9: ${ histogram.getValueAtPercentile(99.9)}
        //max: ${ histogram.maxValue}
        //`;
        //}

        private static string OutputHistogramSummary(HistogramBase histogram)
        {
            return $@"
count: {histogram.TotalCount}
        p50: {histogram.GetValueAtPercentile(50)}
        p90: {histogram.GetValueAtPercentile(90)}
        p99: {histogram.GetValueAtPercentile(99)}
      p99.9: {histogram.GetValueAtPercentile(99.9)}
        max: {histogram.GetMaxValue()}
";
        }

        //private static getElapsedMillis(startTime: [number, number]): number
        //{
        //    const endTime = process.hrtime(startTime);
        //    return (endTime[0] * 1e9 + endTime[1]) / 1e6;
        //}
        //}



    }


    internal class Program
    {
        static ILoggerFactory InitializeLogging()
        {
            return LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "hh:mm:ss ";
                   }));
        }

        const string PERFORMANCE_INFORMATION_MESSAGE = @"
Thanks for trying out our basic c# load generator!  This tool is
included to allow you to experiment with performance in your environment
based on different configurations.  It's very simplistic, and only intended
to give you a quick way to explore the performance of the Momento client
running in a dotnet application.

Since performance will be impacted by network latency, you'll get the best
results if you run on a cloud VM in the same region as your Momento cache.

Check out the configuration settings at the bottom of the 'MomentoLoadGen/Program.cs'
file to see how different configurations impact performance.

If you have questions or need help experimenting further, please reach out to us!
        ";


        static async Task Main(string[] args)
        {
            using ILoggerFactory loggerFactory = InitializeLogging();

            CsharpLoadGeneratorOptions loadGeneratorOptions = new CsharpLoadGeneratorOptions(
              /**
               * Configures the Momento client to timeout if a request exceeds this limit.
               * Momento client default is 5 seconds.
               */
              requestTimeoutMs: 5 * 1000,
              /**
               * Controls the size of the payload that will be used for the cache items in
               * the load test.  Smaller payloads will generally provide lower latencies than
               * larger payloads.
               */
              cacheItemPayloadBytes: 100,
              /**
               * Controls the number of concurrent requests that will be made (via asynchronous
               * function calls) by the load test.  Increasing this number may improve throughput,
               * but it will also increase CPU consumption.  As CPU usage increases and there
               * is more contention between the concurrent function calls, client-side latencies
               * may increase.
               */
              numberOfConcurrentRequests: 50,
              /**
               * Controls how long the load test will run.  We will execute this many operations
               * (1 cache 'set' followed immediately by 1 'get') across all of our concurrent
               * workers before exiting.  Statistics will be logged every 1000 operations.
               */
              totalNumberOfOperationsToExecute: 50_000
            );

            CsharpLoadGenerator loadGenerator = new CsharpLoadGenerator(
                loggerFactory,
                loadGeneratorOptions
                );
            try
            {
                await loadGenerator.Run();
                Console.WriteLine("success!");
                Console.WriteLine(PERFORMANCE_INFORMATION_MESSAGE);
            } catch (Exception e)
            {
                Console.WriteLine("ERROR!: {0}", e);
            }
        }
    }
}