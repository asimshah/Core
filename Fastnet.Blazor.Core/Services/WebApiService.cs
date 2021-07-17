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
using System.Diagnostics;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fastnet.Blazor.Core
{
    public class WebApiResult
    {
        public bool Success { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
        public object Data { get; set; }
    }
    public class WebApiResult<T> : WebApiResult
    {
        public WebApiResult()
        {

        }
        public WebApiResult(bool success, T data, Dictionary<string, string[]> errors)
        {
            this.Success = success;
            base.Data = data;
            this.Errors = errors;
        }
        //public bool Success { get; set; }
        public new T Data => (T)base.Data;
        //public Dictionary<string, string[]> Errors { get; set; }
    }
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
        protected Action onUnauthorised;
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
        protected async Task<WebApiResult<T>> GetAsync<T>(string query/*, ValidatorCollection validators = null*/)
        {
            //if(typeof(T).IsArray)
            //{
            //    T array = await client.GetFromJsonAsync<T>(query);
            //    return new WebApiResult<T>(true, array, null);
            //}
            var response = await client.GetAsync(query);
            return await ProcessResponseAsync<T>(response, query);

        }
        /// <summary>
        /// 
        /// </summary>
        //protected async Task<WebApiResult<object>> PutAsync<T>(string query, T obj/*, ValidatorCollection validators = null*/)
        //{
        //    var response = await client.PutAsJsonAsync<T>(query, obj);
        //    return await ProcessResponseAsync<object>(response, query);
        //}
        protected async Task<WebApiResult> PutAsync<T>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PutAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<object>(response, query);
        }
        /// <summary>
        /// 
        /// </summary>
        protected async Task<WebApiResult<RT>> PutAsync<T, RT>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PutAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<RT>(response, query);
        }
        /// <summary>
        /// 
        /// </summary>
        protected async Task<WebApiResult> DeleteAsync(string query/*, ValidatorCollection validators = null*/)
        {
            var response = await client.DeleteAsync(query);
            return await ProcessResponseAsync(response, query);
        }
        /// <summary>
        /// 
        /// </summary>

        protected async Task<WebApiResult> PostAsync<T>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PostAsJsonAsync<T>(query, obj);
            var war = await ProcessResponseAsync(response, query);

            return war;
        }
        /// <summary>
        /// 
        /// </summary>

        protected async Task<WebApiResult<RT>> PostAsync<T, RT>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PostAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<RT>(response, query);
        }
        public virtual void SetOnUnAuthorised(Action method)
        {
            onUnauthorised = method;
        }
        private async Task<WebApiResult> ProcessResponseAsync(HttpResponseMessage response, string originalQuery, Type dataType = null)
        {
            Dictionary<string, string[]> errorDictionary = new();
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    if (dataType != null)
                    {
                        object item = null;// Activator.CreateInstance(dataType);

                        //var item = default(T);
                        var t = dataType;// typeof(T);
                        if (t.IsEnum)
                        {
                            log.Error($"{t.Name} not yet supported");
                        }
                        else if (t == typeof(DateTime) || t == typeof(DateTimeOffset))
                        {
                            log.Error($"{t.Name} not yet supported");
                        }
                        else if (t == typeof(decimal))
                        {
                            log.Error($"{t.Name} not yet supported");
                        }
                        else if (t == typeof(string))
                        {
                            item = (object)(await response.Content.ReadAsStringAsync());
                        }

                        else // includes value types, primitives, classes, records
                        {
                            item = await response.Content.ReadFromJsonAsync(dataType);
                        }
                        return new WebApiResult { Data = item, Success = true, Errors = null };
                        //return new WebApiResult<T>(true, item, null);
                    }
                    else
                    {
                        return new WebApiResult { Data = null, Success = true, Errors = null };
                    }
                }
                else
                {
                    log.Error($"HttpStatusCode: {response.StatusCode}");
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        errorDictionary.Add("", new string[] { response.ReasonPhrase });
                        onUnauthorised?.Invoke();
                    }
                    else
                    {
                        var modelState = await response.Content.ReadAsStringAsync(); ;
                        errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
                    }
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                return new WebApiResult { Data = null, Success = false, Errors = null };
                //return new WebApiResult<T>(false, default(T), null);
            }
            catch (Exception xe)
            {
                errorDictionary.Add("", new string[] { xe.Message });
                // what should be done here? (is AccessTokenNotAvailableException important?)
                log.Error(xe);
            }
            foreach (var kvp in errorDictionary)
            {
                var errors = string.Join(", ", kvp.Value);
                var key = (kvp.Key == null || kvp.Key == string.Empty) ? "<none>" : kvp.Key;
                log.Error($"{key}: {errors}");
            }
            return new WebApiResult { Data = null, Success = false, Errors = errorDictionary };
            //return new WebApiResult<T>(false, default(T), errorDictionary);
        }
        private async Task<WebApiResult<T>> ProcessResponseAsync<T>(HttpResponseMessage response, string originalQuery)
        {
            var war = await ProcessResponseAsync(response, originalQuery, typeof(T));
            return new WebApiResult<T>(war.Success, (T)war.Data, war.Errors);
            //return (WebApiResult<T>)(object)war;// await ProcessResponseAsync(response, originalQuery, typeof(T)) as WebApiResult<T>;
            //Dictionary<string, string[]> errorDictionary = new();
            //try
            //{
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var item = default(T);
            //        var t = typeof(T);
            //        if (t.IsEnum)
            //        {
            //            log.Error($"{t.Name} not yet supported");
            //        }
            //        else if (t == typeof(DateTime) || t == typeof(DateTimeOffset))
            //        {
            //            log.Error($"{t.Name} not yet supported");
            //        }
            //        else if(t == typeof(decimal))
            //        {
            //            log.Error($"{t.Name} not yet supported");
            //        }
            //        else if(t == typeof(string))
            //        {
            //            item = (T)(object)(await response.Content.ReadAsStringAsync());
            //        }

            //        else // includes value types, primitives, classes, records
            //        {
            //            item = await response.Content.ReadFromJsonAsync<T>();
            //        }

            //        return new WebApiResult<T>(true, item, null);
            //    }
            //    else
            //    {
            //        log.Error($"HttpStatusCode: {response.StatusCode}");
            //        if (response.StatusCode == HttpStatusCode.Unauthorized)
            //        {
            //            errorDictionary.Add("", new string[] { response.ReasonPhrase });
            //            onUnauthorised?.Invoke();
            //        }
            //        else
            //        {
            //            var modelState = await response.Content.ReadAsStringAsync(); ;
            //            errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
            //        }
            //    }
            //}
            //catch (AccessTokenNotAvailableException exception)
            //{
            //    exception.Redirect();
            //    return new WebApiResult<T>(false, default(T), null);
            //}
            //catch (Exception xe)
            //{
            //    errorDictionary.Add("", new string[] { xe.Message });
            //    // what should be done here? (is AccessTokenNotAvailableException important?)
            //    log.Error(xe);
            //}
            //foreach (var kvp in errorDictionary)
            //{
            //    var errors = string.Join(", ", kvp.Value);
            //    var key = (kvp.Key == null || kvp.Key == string.Empty) ? "<none>" : kvp.Key;
            //    log.Error($"{key}: {errors}");
            //}
            //return new WebApiResult<T>(false, default(T), errorDictionary);
        }
    }
}
