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
    //public record WebApiResult<T>(bool Success, T Data, Dictionary<string, string[]> Errors);
    public class WebApiResult<T>
    {
        public WebApiResult()
        {

        }
        public WebApiResult(bool success, T data, Dictionary<string, string[]> errors)
        {
            this.Success = success;
            this.Data = data;
            this.Errors = errors;
        }
        public bool Success { get; set; }
        public T Data { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
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
            if(typeof(T).IsArray)
            {
                T array = await client.GetFromJsonAsync<T>(query);
                return new WebApiResult<T>(true, array, null);
            }
            var response = await client.GetAsync(query);
            return await ProcessResponseAsync<T>(response/*, validators*/);

        }
        /// <summary>
        /// 
        /// </summary>
        protected async Task<WebApiResult<object>> PutAsync<T>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PutAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<object>(response/*, validators*/);
        }
        /// <summary>
        /// 
        /// </summary>
        protected async Task<WebApiResult<RT>> PutAsync<T, RT>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PutAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<RT>(response/*, validators*/);
        }
        /// <summary>
        /// 
        /// </summary>
        protected async Task<WebApiResult<object>> DeleteAsync(string query/*, ValidatorCollection validators = null*/)
        {
            var response = await client.DeleteAsync(query);
            return await ProcessResponseAsync<object>(response/*, validators*/);
        }
        /// <summary>
        /// 
        /// </summary>

        protected async Task<WebApiResult<object>> PostAsync<T>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PostAsJsonAsync<T>(query, obj);
            var war = await ProcessResponseAsync<object>(response/*, validators*/);

            return war;
        }
        /// <summary>
        /// 
        /// </summary>

        protected async Task<WebApiResult<RT>> PostAsync<T, RT>(string query, T obj/*, ValidatorCollection validators = null*/)
        {
            var response = await client.PostAsJsonAsync<T>(query, obj);
            return await ProcessResponseAsync<RT>(response/*, validators*/);
        }
        //protected void AddErrors<T>(IFormValidator formValidator, WebApiResult<T> webResult)
        //{
        //    formValidator.AddErrors(webResult.Errors);
        //}
        //private async Task<WebApiResult<object>> ProcessResponseAsync(HttpResponseMessage response)
        //{
        //    return await ProcessResponseAsync<object>(response);
        //    //Dictionary<string, string[]> errorDictionary = new();
        //    //try
        //    //{
        //    //    if (response.IsSuccessStatusCode)
        //    //    {
        //    //        return;
        //    //    }
        //    //    else
        //    //    {
        //    //        var modelState = await response.Content.ReadAsStringAsync();
        //    //        errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
        //    //    }
        //    //}
        //    //catch (AccessTokenNotAvailableException exception)
        //    //{
        //    //    exception.Redirect();
        //    //    return ;
        //    //}
        //    //catch (Exception xe)
        //    //{
        //    //    errorDictionary.Add("", new string[] { xe.Message });
        //    //    // what should be done here? (is AccessTokenNotAvailableException important?)
        //    //    log.Error(xe);
        //    //}
        //    //throw new WebApiException(errorDictionary);
        //}
        private async Task<WebApiResult<T>> ProcessResponseAsync<T>(HttpResponseMessage response/*, ValidatorCollection validators*/)
        {
            Dictionary<string, string[]> errorDictionary = new();
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var item = default(T);
                    var t = typeof(T);
                    if (t.IsEnum)
                    {
                        log.Error($"{t.Name} not yet supported");
                    }
                    else if (t == typeof(DateTime) || t == typeof(DateTimeOffset))
                    {
                        log.Error($"{t.Name} not yet supported");
                    }
                    else if(t == typeof(decimal))
                    {
                        log.Error($"{t.Name} not yet supported");
                    }
                    //else if(t.IsValueType && t.IsPrimitive)
                    //{
                    //    item = await response.Content.ReadFromJsonAsync<T>();

                    //    log.Error($"{t.Name} not yet supported");
                    //}
                    else if(t == typeof(string))
                    {
                        item = (T)(object)(await response.Content.ReadAsStringAsync());
                    }

                    else // includes value types, primitives, classes, records
                    {
                        item = await response.Content.ReadFromJsonAsync<T>();
                    }

                    return new WebApiResult<T>(true, item, null);
                    //return (true, await response.Content.ReadFromJsonAsync<T>(), null);
                }
                else
                {
                    try
                    {
                        var modelState = await response.Content.ReadAsStringAsync();
                        //log.Information($"modelState is {modelState}");
                        errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
                        //log.Information($"deserialised to a dictionary of {errorDictionary.Count()} items");
                    }
                    catch (Exception xe)
                    {
                        Debugger.Break();
                    }
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                return new WebApiResult<T>(false, default(T), null);
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
            //if (validators != null && validators.Count() > 0)
            //{
            //    foreach(var dv in validators)
            //    {
            //        dv.AddErrors(errorDictionary);
            //    }
            //    //IFormValidator formV = validators.OfType<IFormValidator>().SingleOrDefault();
            //    //formV?.AddErrors(errorDictionary);
            //    //IFieldValidator fieldV = validators.OfType<IFieldValidator>().SingleOrDefault();
            //    //fieldV?.AddErrors(errorDictionary);
            //}
            return new WebApiResult<T>(false, default(T), errorDictionary);
            //throw new WebApiException(errorDictionary);
        }
        //private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response)
        //{
        //    Dictionary<string, string[]> errorDictionary = new();
        //    try
        //    {
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return await response.Content.ReadFromJsonAsync<T>();
        //        }
        //        else
        //        {
        //            try
        //            {
        //                var modelState = await response.Content.ReadAsStringAsync();
        //                log.Information($"modelState is {modelState}");
        //                errorDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(modelState);
        //                log.Information($"deserialised to a dictionary of {errorDictionary.Count()} items");
        //                foreach (var item in errorDictionary)
        //                {
        //                    foreach (var s in item.Value)
        //                    {
        //                        log.Information($"message: {s}");
        //                    }
        //                }
        //            }
        //            catch (Exception xe)
        //            {
        //                Debugger.Break();
        //            }
        //        }
        //    }
        //    catch (AccessTokenNotAvailableException exception)
        //    {
        //        exception.Redirect();
        //        return default(T);
        //    }
        //    catch (Exception xe)
        //    {
        //        errorDictionary.Add("", new string[] { xe.Message });
        //        // what should be done here? (is AccessTokenNotAvailableException important?)
        //        log.Error(xe);
        //    }
        //    throw new WebApiException(errorDictionary);
        //}
    }
}
