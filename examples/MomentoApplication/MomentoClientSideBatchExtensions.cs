using Microsoft.Extensions.Logging.Abstractions;
using Momento.Sdk;
using Momento.Sdk.Exceptions;
using Momento.Sdk.Internal;
using Momento.Sdk.Responses;

public abstract class CacheGetBatchResponse
{
    public class Success : CacheGetBatchResponse
    {
        public List<CacheGetResponse> Responses { get; }

        public Success(IEnumerable<CacheGetResponse> responses)
        {
            this.Responses = new(responses);
        }

        public IEnumerable<string?> ValueStrings
        {
            get
            {
                var ret = new List<string?>();
                foreach (CacheGetResponse response in Responses)
                {
                    if (response is CacheGetResponse.Hit hitResponse)
                    {
                        ret.Add(hitResponse.ValueString);
                    }
                    else if (response is CacheGetResponse.Miss missResponse)
                    {
                        ret.Add(null);
                    }
                }
                return ret.ToArray();
            }
        }

        public IEnumerable<byte[]?> ValueByteArrays
        {
            get
            {
                var ret = new List<byte[]?>();
                foreach (CacheGetResponse response in Responses)
                {
                    if (response is CacheGetResponse.Hit hitResponse)
                    {
                        ret.Add(hitResponse.ValueByteArray);
                    }
                    else if (response is CacheGetResponse.Miss missResponse)
                    {
                        ret.Add(null);
                    }
                }
                return ret.ToArray();
            }
        }
    }

    public class Error : CacheGetBatchResponse
    {
        private readonly SdkException _error;
        public Error(SdkException error)
        {
            _error = error;
        }

        public SdkException Exception
        {
            get => _error;
        }

        public MomentoErrorCode ErrorCode
        {
            get => _error.ErrorCode;
        }

        public string Message
        {
            get => $"{_error.MessageWrapper}: {_error.Message}";
        }

    }
}

public abstract class CacheSetBatchResponse
{

    public class Success : CacheSetBatchResponse { }

    public class Error : CacheSetBatchResponse
    {
        private readonly SdkException _error;
        public Error(SdkException error)
        {
            _error = error;
        }

        public SdkException Exception
        {
            get => _error;
        }

        public MomentoErrorCode ErrorCode
        {
            get => _error.ErrorCode;
        }

        public string Message
        {
            get => $"{_error.MessageWrapper}: {_error.Message}";
        }

    }
}


public static class MomentoClientSideBatchExtensions
{
    private static CacheExceptionMapper _exceptionMapper = new CacheExceptionMapper(new NullLoggerFactory());
    
    public static async Task<CacheGetBatchResponse> GetBatchAsync(this ICacheClient cacheClient, string cacheName, IEnumerable<string> keys)
    {
        try
        {
            Utils.ArgumentNotNull(cacheName, nameof(cacheName));
            Utils.ArgumentNotNull(keys, nameof(keys));
            Utils.ElementsNotNull(keys, nameof(keys));
        }
        catch (ArgumentNullException e)
        {
            return new CacheGetBatchResponse.Error(new InvalidArgumentException(e.Message));
        }

        // Gather the tasks
        var tasks = keys.Select(key => cacheClient.GetAsync(cacheName, key));

        // Run the tasks
        var continuation = Task.WhenAll(tasks);
        try
        {
            await continuation;
        }
        catch (Exception e)
        {
            return new CacheGetBatchResponse.Error(_exceptionMapper.Convert(e));
        }

        // Handle failures
        if (continuation.Status == TaskStatus.Faulted)
        {
            return new CacheGetBatchResponse.Error(
                _exceptionMapper.Convert(continuation.Exception)
            );
        }
        else if (continuation.Status != TaskStatus.RanToCompletion)
        {
            return new CacheGetBatchResponse.Error(
                _exceptionMapper.Convert(
                    new Exception(String.Format("Failure issuing multi-get: {0}", continuation.Status))
                )
            );
        }

        // preserve old behavior of failing on first error
        foreach (CacheGetResponse response in continuation.Result)
        {
            if (response is CacheGetResponse.Error errorResponse)
            {
                return new CacheGetBatchResponse.Error(errorResponse.InnerException);
            }
        }

        // Package results
        return new CacheGetBatchResponse.Success(continuation.Result);
    }


    public static async Task<CacheSetBatchResponse> SetBatchAsync(this ICacheClient cacheClient, string cacheName,
        IEnumerable<KeyValuePair<string, string>> items, TimeSpan? ttl = null)
    {
        try
        {
            Utils.ArgumentNotNull(cacheName, nameof(cacheName));
            Utils.ArgumentNotNull(items, nameof(items));
            Utils.KeysAndValuesNotNull(items, nameof(items));
        }
        catch (ArgumentNullException e)
        {
            return new CacheSetBatchResponse.Error(new InvalidArgumentException(e.Message));
        }

        // Gather the tasks
        var tasks = items.Select(item => cacheClient.SetAsync(cacheName, item.Key, item.Value, ttl));

        // Run the tasks
        var continuation = Task.WhenAll(tasks);
        try
        {
            await continuation;
        }
        catch (Exception e)
        {
            return new CacheSetBatchResponse.Error(
                _exceptionMapper.Convert(e)
            );
        }

        // Handle failures
        if (continuation.Status == TaskStatus.Faulted)
        {
            return new CacheSetBatchResponse.Error(
                _exceptionMapper.Convert(continuation.Exception)
            );
        }
        else if (continuation.Status != TaskStatus.RanToCompletion)
        {
            return new CacheSetBatchResponse.Error(
                _exceptionMapper.Convert(
                    new Exception(String.Format("Failure issuing multi-set: {0}", continuation.Status))
                )
            );
        }

        return new CacheSetBatchResponse.Success();
    }
}