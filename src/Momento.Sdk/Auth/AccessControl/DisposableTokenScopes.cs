using System;
using System.Collections.Generic;

namespace Momento.Sdk.Auth.AccessControl;

public record DisposableTokenScopes(List<DisposableTokenPermission> Permissions)
{
    public static DisposableTokenScope CacheReadWrite(string cacheName)
    {
        return new DisposableTokenScope(Permissions: new List<DisposableTokenPermission>
        {
            new DisposableTokenPermission.CacheItemPermission(
                CacheRole.ReadWrite,
                CacheSelector.ByName(cacheName),
                CacheItemSelector.AllCacheItems
            )
        });
    }
}