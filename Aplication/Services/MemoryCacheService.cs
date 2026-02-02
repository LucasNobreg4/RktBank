using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T? Get<T>(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            if (absoluteExpiration.HasValue)
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            }

            if (slidingExpiration.HasValue)
            {
                cacheOptions.SlidingExpiration = slidingExpiration;
            }

            if (!absoluteExpiration.HasValue && !slidingExpiration.HasValue)
            {
                cacheOptions.SlidingExpiration = TimeSpan.FromMinutes(5);
            }

            _memoryCache.Set(key, value, cacheOptions);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            Set(key, value, absoluteExpiration, slidingExpiration);
            return Task.CompletedTask;
        }

        public void SetPermanent<T>(string key, T value)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };
            _memoryCache.Set(key, value, cacheOptions);
        }

        public Task SetPermanentAsync<T>(string key, T value)
        {
            SetPermanent(key, value);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}