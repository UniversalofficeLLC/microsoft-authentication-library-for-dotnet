﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Cache;
using Microsoft.Identity.Client.Utils;

namespace Microsoft.Identity.Client
{
    #if !ANDROID_BUILDTIME && !iOS_BUILDTIME && !WINDOWS_APP_BUILDTIME
    /// <summary>
    /// Extension methods used to subscribe to cache serialization events, and to effectively serialize and deserialize the cache
    /// </summary>
    /// <remarks>New in MSAL.NET 2.x: it's now possible to deserialize the token cache in two formats, the ADAL V3 legacy token cache
    /// format, and the new unified cache format, common to ADAL.NET, MSAL.NET, and other libraries on the same platform (MSAL.objc, on iOS)</remarks>
    public static class TokenCacheExtensions
    {
        private const string AccessTokenKey = "access_tokens";
        private const string RefreshTokenKey = "refresh_tokens";
        private const string IdTokenKey = "id_tokens";
        private const string AccountKey = "accounts";

        /// <summary>
        /// Sets a delegate to be notified before any library method accesses the cache. This gives an option to the
        /// delegate to deserialize a cache entry for the application and accounts specified in the <see cref="TokenCacheNotificationArgs"/>.
        /// See https://aka.ms/msal-net-token-cache-serialization
        /// </summary>
        /// <param name="tokenCache">Token cache that will be accessed</param>
        /// <param name="beforeAccess">Delegate set in order to handle the cache deserialiation</param>
        /// <remarks>In the case where the delegate is used to deserialize the cache, it might
        /// want to call <see cref="Deserialize(TokenCache, byte[])"/></remarks>
        public static void SetBeforeAccess(this TokenCache tokenCache, TokenCache.TokenCacheNotification beforeAccess)
        {
            GuardOnMobilePlatforms();
            tokenCache.BeforeAccess = beforeAccess;
        }

        /// <summary>
        /// Sets a delegate to be notified after any library method accesses the cache. This gives an option to the
        /// delegate to serialize a cache entry for the application and accounts specified in the <see cref="TokenCacheNotificationArgs"/>.
        /// See https://aka.ms/msal-net-token-cache-serialization
        /// </summary>
        /// <param name="tokenCache">Token cache that was accessed</param>
        /// <param name="afterAccess">Delegate set in order to handle the cache serialization in the case where the <see cref="TokenCache.HasStateChanged"/>
        /// member of the cache is <c>true</c></param>
        /// <remarks>In the case where the delegate is used to serialize the cache entierely (not just a row), it might
        /// want to call <see cref="Serialize(TokenCache)"/></remarks>
        public static void SetAfterAccess(this TokenCache tokenCache, TokenCache.TokenCacheNotification afterAccess)
        {
            GuardOnMobilePlatforms();
            tokenCache.AfterAccess = afterAccess;
        }

        /// <summary>
        /// Sets a delegate called before any library method writes to the cache. This gives an option to the delegate
        /// to reload the cache state from a row in database and lock that row. That database row can then be unlocked in the delegate
        /// registered with <see cref="SetAfterAccess(TokenCache, TokenCache.TokenCacheNotification)"/>
        /// </summary>
        /// <param name="tokenCache">Token cache that will be accessed</param>
        /// <param name="beforeWrite">Delegate set in order to prepare the cache serialization</param>
        public static void SetBeforeWrite(this TokenCache tokenCache, TokenCache.TokenCacheNotification beforeWrite)
        {
            GuardOnMobilePlatforms();
            tokenCache.BeforeWrite = beforeWrite;
        }

        /// <summary>
        /// Deserializes the token cache from a serialization blob in the unified cache format
        /// </summary>
        /// <param name="tokenCache">Token cache to deserialize (to fill-in from the state)</param>
        /// <param name="unifiedState">Array of bytes containing serialized Msal cache data</param>
        /// <remarks>
        /// <paramref name="unifiedState"/>Is a Json blob containing access tokens, refresh tokens, id tokens and accounts information.
        /// </remarks>
        public static void Deserialize(this TokenCache tokenCache, byte[] unifiedState)
        {
            GuardOnMobilePlatforms();
            lock (tokenCache.LockObject)
            {
                var requestContext = tokenCache.CreateRequestContext();
                tokenCache.TokenCacheAccessor.Clear();

                Dictionary<string, IEnumerable<string>> cacheDict = JsonHelper
                    .DeserializeFromJson<Dictionary<string, IEnumerable<string>>>(unifiedState);

                if (cacheDict == null || cacheDict.Count == 0)
                {
                    tokenCache.Logger.Info("Msal Cache is empty.");
                    return;
                }

                if (cacheDict.ContainsKey(AccessTokenKey))
                {
                    foreach (var atItem in cacheDict[AccessTokenKey])
                    {
                        var msalAccessTokenCacheItem =
                            JsonHelper.TryToDeserializeFromJson<MsalAccessTokenCacheItem>(atItem, requestContext);
                        if (msalAccessTokenCacheItem != null)
                        {
                            tokenCache.TokenCacheAccessor.SaveAccessToken(msalAccessTokenCacheItem);
                        }
                    }
                }

                if (cacheDict.ContainsKey(RefreshTokenKey))
                {
                    foreach (var rtItem in cacheDict[RefreshTokenKey])
                    {
                        var msalRefreshTokenCacheItem =
                            JsonHelper.TryToDeserializeFromJson<MsalRefreshTokenCacheItem>(rtItem, requestContext);
                        if (msalRefreshTokenCacheItem != null)
                        {
                            tokenCache.TokenCacheAccessor.SaveRefreshToken(msalRefreshTokenCacheItem);
                        }
                    }
                }

                if (cacheDict.ContainsKey(IdTokenKey))
                {
                    foreach (var idItem in cacheDict[IdTokenKey])
                    {
                        var msalIdTokenCacheItem =
                            JsonHelper.TryToDeserializeFromJson<MsalIdTokenCacheItem>(idItem, requestContext);
                        if (msalIdTokenCacheItem != null)
                        {
                            tokenCache.TokenCacheAccessor.SaveIdToken(msalIdTokenCacheItem);
                        }
                    }
                }

                if (cacheDict.ContainsKey(AccountKey))
                {
                    foreach (var account in cacheDict[AccountKey])
                    {
                        var msalAccountCacheItem =
                            JsonHelper.TryToDeserializeFromJson<MsalAccountCacheItem>(account, requestContext);

                        if (msalAccountCacheItem != null)
                        {
                            tokenCache.TokenCacheAccessor.SaveAccount(msalAccountCacheItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes the token cache from a serialization blob in both format (ADAL V3 format, and unified cache format)
        /// </summary>
        /// <param name="tokenCache">Token cache to deserialize (to fill-in from the state)</param>
        /// <param name="cacheData">Array of bytes containing serialicache data</param>
        public static void DeserializeUnifiedAndAdalCache(this TokenCache tokenCache, CacheData cacheData)
        {
            GuardOnMobilePlatforms();
            lock (tokenCache.LockObject)
            {
                Deserialize(tokenCache, cacheData.UnifiedState);
                tokenCache.LegacyCachePersistence.WriteCache(cacheData.AdalV3State);
            }
        }

        /// <summary>
        /// Serializes the entire token cache, in the unified cache format only
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize</param>
        /// <returns>array of bytes containing the serialized unified cache</returns>
        public static byte[] Serialize(this TokenCache tokenCache)
        {
            GuardOnMobilePlatforms();
            // reads the underlying in-memory dictionary and dumps out the content as a JSON
            lock (tokenCache.LockObject)
            {
                // reads the underlying in-memory dictionary and dumps out the content as a JSON
                Dictionary<string, IEnumerable<string>> cacheDict = new Dictionary<string, IEnumerable<string>>
                {
                    [AccessTokenKey] = tokenCache.TokenCacheAccessor.GetAllAccessTokensAsString(),
                    [RefreshTokenKey] = tokenCache.TokenCacheAccessor.GetAllRefreshTokensAsString(),
                    [IdTokenKey] = tokenCache.TokenCacheAccessor.GetAllIdTokensAsString(),
                    [AccountKey] = tokenCache.TokenCacheAccessor.GetAllAccountsAsString()
                };

                return JsonHelper.SerializeToJson(cacheDict).ToByteArray();
            }
        }

        /// <summary>
        /// Serializes the entire token cache in both the ADAL V3 and unified cache formats.
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize</param>
        /// <returns>Serialized token cache <see cref="CacheData"/></returns>
        public static CacheData SerializeUnifiedAndAdalCache(this TokenCache tokenCache)
        {
            GuardOnMobilePlatforms();
            // reads the underlying in-memory dictionary and dumps out the content as a JSON
            lock (tokenCache.LockObject)
            {
                var serializedUnifiedCache = Serialize(tokenCache);
                var serializeAdalCache = tokenCache.LegacyCachePersistence.LoadCache();

                return new CacheData()
                {
                    AdalV3State = serializeAdalCache,
                    UnifiedState = serializedUnifiedCache
                };
            }
        }
        
        private static void GuardOnMobilePlatforms()
        {
#if ANDROID || iOS || WINDOWS_APP
        throw new PlatformNotSupportedException("You should not use these TokenCache methods object on mobile platforms. " +
            "They meant to allow applications to define their own storage strategy on .net desktop and .net core. " +
            "On mobile platforms, a secure and performant storage mechanism is implemeted by MSAL. " +
            "For more details about custom token cache serialization, visit https://aka.ms/msal-net-serialization");
#endif
        }
    }
    #endif // !ANDROID_BUILDTIME && !iOS_BUILDTIME && !WINDOWS_APP_BUILDTIME
}
