using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace aspnet.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDistributedCache _cache;
    private readonly IMemoryCache _memoryCache;

    public IndexModel(ILogger<IndexModel> logger, IDistributedCache cache, IMemoryCache memoryCache)
    {
        _logger = logger;
        _cache = cache;
        _memoryCache = memoryCache;
    }

    public void OnGet()
    {
        _logger.LogInformation($"INDEX PAGE: GET: the cache is: {_cache}");

        for (int i = 0; i < 100; i++)
        {
            _logger.LogInformation($"Adding an item to the memory cache ({i})");
            String setResult = _memoryCache.Set($"foo{i}", $"FOOOOO{i}!", new MemoryCacheEntryOptions()
            {
                Size = 1
            });
            _logger.LogInformation($"Set result ({i}): {setResult}");
            MemoryCacheStatistics stats = _memoryCache.GetCurrentStatistics();
            _logger.LogInformation($"Memory cache stats: count: {stats.CurrentEntryCount}, size: {stats.CurrentEstimatedSize}");
            String? foo1;
            bool gotFoo1 = _memoryCache.TryGetValue("foo1", out foo1);
            _logger.LogInformation($"Got foo1? {gotFoo1}");
            String? foo9;
            bool gotFoo9 = _memoryCache.TryGetValue("foo9", out foo9);
            _logger.LogInformation($"Got foo9? {gotFoo9}");
            String? foo99;
            bool gotFoo99 = _memoryCache.TryGetValue("foo99", out foo99);
            _logger.LogInformation($"Got foo99? {gotFoo99}");
        }
        // _cache.SetString();
    }
}
