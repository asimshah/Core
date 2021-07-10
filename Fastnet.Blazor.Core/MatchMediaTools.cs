using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Fastnet.Blazor.Core
{
    public class MatchMediaTools : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        public MatchMediaTools(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                 "import", "./_content/Fastnet.Blazor.Core/matchMedia.js").AsTask());
        }
        public async ValueTask<IJSObjectReference> NotifyChanges()
        {
            var module = await moduleTask.Value;
            var jsObj = await module.InvokeAsync<IJSObjectReference>("mm_notifyChange");

            return jsObj;
        }
        public async ValueTask RemoveNotification(IJSObjectReference mq)
        {

            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("mm_removeNofication", mq);
        }
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
