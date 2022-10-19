﻿using System.Threading.Tasks;
using Momento.Sdk.Auth;

namespace Momento.Sdk.Tests;

[Collection("SimpleCacheClient")]
public class SimpleCacheDataTest
{
    private readonly ICredentialProvider authProvider;
    private readonly string cacheName;
    private TimeSpan defaultTtl;
    private SimpleCacheClient client;

    // Test initialization
    public SimpleCacheDataTest(SimpleCacheClientFixture fixture)
    {
        client = fixture.Client;
        authProvider = fixture.AuthProvider;
        cacheName = fixture.CacheName;

        Console.WriteLine($"\n\n\nUSING CACHE NAME: '{cacheName}'\n\n\n");

        defaultTtl = fixture.DefaultTtl;
    }

    [Theory]
    [InlineData(null, new byte[] { 0x00 }, new byte[] { 0x00 })]
    [InlineData("cache", null, new byte[] { 0x00 })]
    [InlineData("cache", new byte[] { 0x00 }, null)]
    public async Task SetAsync_NullChecksByteArrayByteArray_ThrowsException(string cacheName, byte[] key, byte[] value)
    {
        CacheSetResponse response = await client.SetAsync(cacheName, key, value);
        Assert.True(response is CacheSetResponse.Error);
        var errorResponse = (CacheSetResponse.Error)response;
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, errorResponse.ErrorCode);

        response = await client.SetAsync(cacheName, key, value, defaultTtl);
        Assert.True(response is CacheSetResponse.Error);
        errorResponse = (CacheSetResponse.Error)response;
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, errorResponse.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetAsync_InvalidTTLByteArrayByteArray_ThrowsException(int ttlSeconds)
    {
        CacheSetResponse response = await client.SetAsync(cacheName, new byte[] { 0x00 }, new byte[] { 0x00 }, TimeSpan.FromSeconds(ttlSeconds));
        Assert.True(response is CacheSetResponse.Error);
        var errorResponse = (CacheSetResponse.Error)response;
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, errorResponse.ErrorCode);
    }

    // Tests SetAsyc(cacheName, byte[], byte[]) as well as GetAsync(cacheName, byte[])
    [Fact]
    public async Task SetAsync_KeyIsByteArrayValueIsByteArray_HappyPath()
    {
        byte[] key = Utils.NewGuidByteArray();
        byte[] value = Utils.NewGuidByteArray();
        await client.SetAsync(cacheName, key, value);
        CacheGetResponse response = await client.GetAsync(cacheName, key);
        Assert.True(response is CacheGetResponse.Hit);
        var goodResponse = (CacheGetResponse.Hit)response;
        byte[] setValue = goodResponse.ValueByteArray;
        Assert.Equal(value, setValue);

        key = Utils.NewGuidByteArray();
        value = Utils.NewGuidByteArray();
        await client.SetAsync(cacheName, key, value, ttl: TimeSpan.FromSeconds(15));
        response = await client.GetAsync(cacheName, key);
        Assert.True(response is CacheGetResponse.Hit);
        goodResponse = (CacheGetResponse.Hit)response;
        setValue = goodResponse.ValueByteArray;
        Assert.Equal(value, setValue);
    }

    [Theory]
    [InlineData(null, new byte[] { 0x00 })]
    [InlineData("cache", null)]
    public async Task GetAsync_NullChecksByteArray_ThrowsException(string cacheName, byte[] key)
    {
        CacheGetResponse response = await client.GetAsync(cacheName, key);
        Assert.True(response is CacheGetResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheGetResponse.Error)response).ErrorCode);
    }

    [Theory]
    [InlineData(null, "key", "value")]
    [InlineData("cache", null, "value")]
    [InlineData("cache", "key", null)]
    public async Task SetAsync_NullChecksStringString_ThrowsException(string cacheName, string key, string value)
    {
        CacheSetResponse response = await client.SetAsync(cacheName, key, value);
        Assert.True(response is CacheSetResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheSetResponse.Error)response).ErrorCode);

        response = await client.SetAsync(cacheName, key, value, defaultTtl);
        Assert.True(response is CacheSetResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheSetResponse.Error)response).ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetAsync_InvalidTTLStringString_ThrowsException(int ttlSeconds)
    {
        CacheSetResponse response = await client.SetAsync(cacheName, "hello", "world", TimeSpan.FromSeconds(ttlSeconds));
        Assert.True(response is CacheSetResponse.Error);
        var errorResponse = (CacheSetResponse.Error)response;
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, errorResponse.ErrorCode);
    }

    // Also tests GetAsync(cacheName, string)
    [Fact]
    public async Task SetAsync_KeyIsStringValueIsString_HappyPath()
    {
        // Set without TTL
        string key = Utils.NewGuidString();
        string value = Utils.NewGuidString();
        var setResponse = await client.SetAsync(cacheName, key, value);
        Assert.True(setResponse is CacheSetResponse.Success, $"Expected a Miss but got: {setResponse}");

        CacheGetResponse response = await client.GetAsync(cacheName, key);
        Assert.True(response is CacheGetResponse.Hit);
        var goodResponse = (CacheGetResponse.Hit)response;
        string setValue = goodResponse.ValueString;
        Assert.Equal(value, setValue);

        // Set with TTL
        key = Utils.NewGuidString();
        value = Utils.NewGuidString();
        setResponse = await client.SetAsync(cacheName, key, value, ttl: TimeSpan.FromSeconds(15));
        Assert.True(setResponse is CacheSetResponse.Success);

        response = await client.GetAsync(cacheName, key);
        Assert.True(response is CacheGetResponse.Hit);
        goodResponse = (CacheGetResponse.Hit)response;
        setValue = goodResponse.ValueString;
        Assert.Equal(value, setValue);
    }

    [Theory]
    [InlineData(null, "key")]
    [InlineData("cache", null)]
    public async Task GetAsync_NullChecksString_ThrowsException(string cacheName, string key)
    {
        CacheGetResponse response = await client.GetAsync(cacheName, key);
        Assert.True(response is CacheGetResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheGetResponse.Error)response).ErrorCode);
    }

    [Theory]
    [InlineData(null, "key", new byte[] { 0x00 })]
    [InlineData("cache", null, new byte[] { 0x00 })]
    [InlineData("cache", "key", null)]
    public async Task SetAsync_NullChecksStringByteArray_ThrowsException(string cacheName, string key, byte[] value)
    {
        CacheSetResponse response = await client.SetAsync(cacheName, key, value);
        Assert.True(response is CacheSetResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheSetResponse.Error)response).ErrorCode);

        response = await client.SetAsync(cacheName, key, value, defaultTtl);
        Assert.True(response is CacheSetResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheSetResponse.Error)response).ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetAsync_InvalidTTLStringByteArray_ThrowsException(int ttlSeconds)
    {
        CacheSetResponse response = await client.SetAsync(cacheName, "hello", new byte[] { 0x00 }, TimeSpan.FromSeconds(ttlSeconds));
        Assert.True(response is CacheSetResponse.Error);
        var errorResponse = (CacheSetResponse.Error)response;
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, errorResponse.ErrorCode);
    }

    // Also tests GetAsync(cacheName, string)
    [Fact]
    public async Task SetAsync_KeyIsStringValueIsByteArray_HappyPath()
    {
        // Set without TTL
        string key = Utils.NewGuidString();
        byte[] value = Utils.NewGuidByteArray();
        var setResponse = await client.SetAsync(cacheName, key, value);
        Assert.True(setResponse is CacheSetResponse.Success);

        CacheGetResponse response = await client.GetAsync(cacheName, key);
        var goodResponse = (CacheGetResponse.Hit)response;
        byte[] setValue = goodResponse.ValueByteArray;
        Assert.Equal(value, setValue);

        // Set with TTL
        key = Utils.NewGuidString();
        value = Utils.NewGuidByteArray();
        setResponse = await client.SetAsync(cacheName, key, value, ttl: TimeSpan.FromSeconds(15));
        Assert.True(setResponse is CacheSetResponse.Success);

        response = await client.GetAsync(cacheName, key);
        var anotherGoodResponse = (CacheGetResponse.Hit)response;
        byte[] anotherSetValue = anotherGoodResponse.ValueByteArray;
        Assert.Equal(value, anotherSetValue);
    }

    [Fact]
    public async Task GetAsync_ExpiredTtl_HappyPath()
    {
        string key = Utils.NewGuidString();
        string value = Utils.NewGuidString();
        var setResponse = await client.SetAsync(cacheName, key, value, TimeSpan.FromSeconds(1));
        Assert.True(setResponse is CacheSetResponse.Success);
        await Task.Delay(3000);
        CacheGetResponse result = await client.GetAsync(cacheName, key);
        Assert.True(result is CacheGetResponse.Miss);
    }

    [Theory]
    [InlineData(null, new byte[] { 0x00 })]
    [InlineData("cache", null)]
    public async Task DeleteAsync_NullChecksByte_ThrowsException(string cacheName, byte[] key)
    {
        CacheDeleteResponse result = await client.DeleteAsync(cacheName, key);
        Assert.True(result is CacheDeleteResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheDeleteResponse.Error)result).ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_KeyIsByteArray_HappyPath()
    {
        // Set a key to then delete
        byte[] key = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        byte[] value = new byte[] { 0x05, 0x06, 0x07, 0x08 };
        var setResponse = await client.SetAsync(cacheName, key, value, ttl: TimeSpan.FromMinutes(1));
        Assert.True(setResponse is CacheSetResponse.Success);
        CacheGetResponse getResponse = await client.GetAsync(cacheName, key);
        Assert.True(getResponse is CacheGetResponse.Hit);

        // Delete
        var deleteResponse = await client.DeleteAsync(cacheName, key);
        Assert.True(deleteResponse is CacheDeleteResponse.Success);

        // Check deleted
        getResponse = await client.GetAsync(cacheName, key);
        Assert.True(getResponse is CacheGetResponse.Miss, $"Expected a Miss but got: {getResponse}");
    }

    [Theory]
    [InlineData(null, "key")]
    [InlineData("cache", null)]
    public async Task DeleteAsync_NullChecksString_ThrowsException(string cacheName, string key)
    {
        CacheDeleteResponse response = await client.DeleteAsync(cacheName, key);
        Assert.True(response is CacheDeleteResponse.Error);
        Assert.Equal(MomentoErrorCode.INVALID_ARGUMENT_ERROR, ((CacheDeleteResponse.Error)response).ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_KeyIsString_HappyPath()
    {
        // Set a key to then delete
        string key = Utils.NewGuidString();
        string value = Utils.NewGuidString();
        var setResponse = await client.SetAsync(cacheName, key, value, ttl: TimeSpan.FromMinutes(1));
        Assert.True(setResponse is CacheSetResponse.Success);
        CacheGetResponse getResponse = await client.GetAsync(cacheName, key);
        Assert.True(getResponse is CacheGetResponse.Hit);

        // Delete
        await client.DeleteAsync(cacheName, key);

        // Check deleted
        getResponse = await client.GetAsync(cacheName, key);
        Assert.True(getResponse is CacheGetResponse.Miss);
    }
}
