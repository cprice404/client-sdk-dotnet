using System.Text;

namespace Momento.Sdk.Auth.AccessControl;



public enum CacheRole
{
    ReadWrite,
    ReadOnly,
    WriteOnly
}

public abstract record CacheSelector
{
    public record SelectByCacheName(string CacheName) : CacheSelector;

    public static SelectByCacheName ByName(string cacheName)
    {
        return new SelectByCacheName(cacheName);
    }
}


public abstract record CacheItemSelector
{
    public record SelectAllCacheItems : CacheItemSelector;

    public static SelectAllCacheItems AllCacheItems = new SelectAllCacheItems();

    public record SelectByKey(string CacheKey) : CacheItemSelector;

    public static SelectByKey ByKey(string cacheKey)
    {
        return new SelectByKey(cacheKey);
    }
}
