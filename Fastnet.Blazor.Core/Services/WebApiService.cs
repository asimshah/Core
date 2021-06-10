using Fastnet.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WebApiService
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly HttpClient client;
        /// <summary>
        /// 
        /// </summary>
        protected readonly ILogger log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public WebApiService(HttpClient client, ILogger logger)
        {
            this.client = client;
            this.log = logger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        protected async Task<T> GetAsync<T>(string query)
        {
            var response = await client.GetAsync(query);
            return await ProcessResponseAsync<T>(response);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="WebApiException"></exception>
        protected async Task PutAsync<T>(string query, T obj)
        {
            var response =  await client.PutAsJsonAsync<T>(query, obj);
            await ProcessResponseAsync(response);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="WebApiException"></exception>
        protected async Task<RT> PutAsync<T,RT>(string query, T obj)
        {
            var response = await client.PutAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<RT>(response);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="WebApiException"></exception>
        protected async Task PostAsync<T>(string query, T obj)
        {
            var response = await client.PostAsJsonAsync<T>(query, obj);
            await ProcessResponseAsync(response);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="WebApiException"></exception>
        protected async Task<RT> PostAsync<T, RT>(string query, T obj)
        {
            var response = await client.PostAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<RT>(response);
        }
        private async Task ProcessResponseAsync(HttpResponseMessage response)
        {
            Dictionary<string, string[]> errorDictionary = new();
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else
                {
                    var modelState = await response.Content.ReadAsStringAsync();
                    errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                return ;
            }
            catch (Exception xe)
            {
                errorDictionary.Add("", new string[] { xe.Message });
                // what should be done here? (is AccessTokenNotAvailableException important?)
                log.Error(xe);
            }
            throw new WebApiException(errorDictionary);
        }
        private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response)
        {
            Dictionary<string, string[]> errorDictionary = new();
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                else
                {
                    var modelState = await response.Content.ReadAsStringAsync();
                    errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                return default(T);
            }
            catch (Exception xe)
            {
                errorDictionary.Add("", new string[] { xe.Message });
                // what should be done here? (is AccessTokenNotAvailableException important?)
                log.Error(xe);
            }
            throw new WebApiException(errorDictionary);
        }
    }
}
