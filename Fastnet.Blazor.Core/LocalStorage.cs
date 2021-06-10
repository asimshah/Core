using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// Manages strongly typed access to browser local storage. Uses System.Text.Json to serialise/deserialise
    /// non string data.
    /// </summary>
    public class LocalStorage : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsRuntime"></param>
        public LocalStorage(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fastnet.Blazor.Core/localStorageScript.js").AsTask());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask ClearAsync()
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("localStorage_clear");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async ValueTask SetAsync<T>(string key, T data)
        {
            string text = data switch
            {
                string s => s,
                _ => JsonSerializer.Serialize<T>(data)
            };
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("localStorage_set", key, text);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async ValueTask<T> GetAsync<T>(string key)
        {
            var module = await moduleTask.Value;
            var text = await module.InvokeAsync<string>("localStorage_get", key);
            if (typeof(T) == typeof(string))
            {
                return (T)(object)text;
            }
            T data = JsonSerializer.Deserialize<T>(text);
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async ValueTask RemoveAsync(string key)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("localStorage_remove", key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async ValueTask<bool> ContainsKey(string key)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<bool>("localStorage_contains", key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
