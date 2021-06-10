using System;
using System.Collections.Generic;

namespace Fastnet.Core.Shared
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public enum ApiResultType
    {
        Success,
        Error
    }
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public record ApiResult
    {
        public ApiResultType Type { get;  init; } = ApiResultType.Error;
        public Dictionary<string, IEnumerable<string>> Errors { get; init; } = new();
        public string Data { get; init; }

    }
}
