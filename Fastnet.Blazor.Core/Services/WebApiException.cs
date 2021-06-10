using System.Collections.Generic;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class WebApiException : System.Exception
    {
        private Dictionary<string, string[]> errorDictionary = new();
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string[]> Errors => errorDictionary;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorDictionary"></param>
        public WebApiException(Dictionary<string, string[]> errorDictionary)
        {
            this.errorDictionary = errorDictionary;
        }
    }
}
