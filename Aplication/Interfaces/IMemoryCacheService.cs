using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMemoryCacheService
    {
        T? Get<T>(string key);
        Task<T?> GetAsync<T>(string key);
        bool TryGetValue<T>(string key, out T? value);
        void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        void SetPermanent<T>(string key, T value);
        Task SetPermanentAsync<T>(string key, T value);
        void Remove(string key);
        Task RemoveAsync(string key);
    }
}
