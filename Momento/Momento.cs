﻿using System;
using ControlClient;
using CacheClient;
using static CacheClient.Scs; 
using static ControlClient.ScsControl;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using MomentoSdk.Exceptions;

namespace MomentoSdk
{
    public class Momento
    {
        private readonly string cacheEndpoint;
        private readonly string authToken;
        private readonly ScsControlClient client;

        public Momento(string authToken)
        {
            Claims claims = JwtUtils.decodeJwt(authToken);
            GrpcChannel channel = GrpcChannel.ForAddress(claims.controlEndpoint, new GrpcChannelOptions() { Credentials = ChannelCredentials.SecureSsl });
            CallInvoker invoker = channel.Intercept(new AuthHeaderInterceptor(authToken));
            this.client = new ScsControlClient(invoker);
            this.authToken = authToken;
            this.cacheEndpoint = claims.cacheEndpoint;
        }

        /// <summary>
        /// Creates a cache if it doesnt exist. Returns the cache.
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="defaultTtlSeconds"></param>
        /// <returns></returns>
        public MomentoCache CreateOrGetCache(String cacheName, uint defaultTtlSeconds)
        {
            try
            {
                CreateCache(cacheName);
                // swallow this error since the cache is already created
            } catch(CacheAlreadyExistsException)
            {
            }
            return GetCache(cacheName, defaultTtlSeconds);

        }

        /// <summary>
        /// Creates a cache with the given name
        /// </summary>
        /// <param name="cacheName"></param>
        public void CreateCache (String cacheName)
        {
            CheckValidCacheName(cacheName);
            try
            {
                CreateCacheRequest request = new CreateCacheRequest() { CacheName = cacheName };
                this.client.CreateCache(request);
            }
            catch (Grpc.Core.RpcException e)
            {
                if (e.StatusCode == StatusCode.AlreadyExists)
                {
                    throw new CacheAlreadyExistsException("cache with name " + cacheName + " already exists");
                }
            }
        }

        /// <summary>
        /// Gets an instance of MomentoCache to perform gets and sets on
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="defaultTtlSeconds"></param>
        /// <returns></returns>
        public MomentoCache GetCache(String cacheName, uint defaultTtlSeconds)
        {
            CheckValidCacheName(cacheName);
            return MomentoCache.Init(this.authToken, cacheName, this.cacheEndpoint, defaultTtlSeconds);
        }

        /// <summary>
        /// Deletes a cache and all of the items within it
        /// </summary>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        public DeleteCacheResponse DeleteCache(String cacheName)
        {
            DeleteCacheRequest request = new DeleteCacheRequest() { CacheName = cacheName };
            return this.client.DeleteCache(request);
        }

        private Boolean CheckValidCacheName(String cacheName)
        {
            if (String.IsNullOrWhiteSpace(cacheName))
            {
                throw new InvalidCacheNameException("cache name must be nonempty");
            }
            return true;
        }

    }
}
