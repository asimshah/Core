using Fastnet.Core.Logging;
//using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace Fastnet.Core.Web.Logging
{
    //[Obsolete("use RollingFileLoggerProvider instead")]
    //internal class RollingFileLoggerWebProvider : RollingFileLoggerProvider // ILoggerProvider
    //{
    //    private IHostEnvironment _env;
    //   // private IHostingEnvironment _env;
    //    public RollingFileLoggerWebProvider(IServiceProvider sp, IOptionsMonitor<RollingFileLoggerOptions> options) : base(options)
    //    {
    //        Debug.Assert(sp != null);
    //        //_env = sp.GetService<IHostingEnvironment>();
    //        _env = sp.GetService<IHostEnvironment>();
    //        Debug.Assert(_env != null);
    //    }
    //    public RollingFileLoggerWebProvider(IServiceProvider sp, Func<string, LogLevel, bool> filter, bool includeScopes) : base(filter, includeScopes)
    //    {
    //        Debug.Assert(sp != null);
    //        // _env = sp.GetService<IHostingEnvironment>();
    //        _env = sp.GetService<IHostEnvironment>();
    //        Debug.Assert(_env != null);
    //    }
    //    public RollingFileLoggerWebProvider(IServiceProvider sp, IRollingFileLoggerSettings settings) : base(settings)
    //    {
    //        Debug.Assert(sp != null);
    //        //_env = sp.GetService<IHostingEnvironment>();
    //        _env = sp.GetService<IHostEnvironment>();
    //        Debug.Assert(_env != null);
    //    }
    //    protected override RollingFileLogger CreateLoggerImplementation(string name)
    //    {
    //        string basePath = _env.ContentRootPath; // _settings?.AppFolder ?? 
    //        if (string.Compare(basePath, "default", true) == 0)
    //        {
    //            return base.CreateLoggerImplementation(name);
    //        }
    //        else
    //        {
    //            return base.CreateLoggerImplementation(name, basePath);
    //        }
    //    }
    //}
}
