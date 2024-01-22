using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;

namespace aspnet.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDistributedCache _cache;

    public IndexModel(ILogger<IndexModel> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public void OnGet()
    {
        _logger.LogInformation($"INDEX PAGE: GET: the cache is: {_cache}");
    }
}
