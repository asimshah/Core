using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class JSTools : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsRuntime"></param>
        public JSTools(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fastnet.Blazor.Core/toolsScript.js").AsTask());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async ValueTask CopyToClipboard(string text)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("tools_clipboardCopy", text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="base64Bytes"></param>
        /// <returns></returns>
        public async ValueTask SaveAsFile(string filename, string base64Bytes)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("tools_saveAsFile", filename, base64Bytes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask<string> GetUserAgent()
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("tools_getUserAgent");
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
