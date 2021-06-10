using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("settings")]
    public class SettingsController : RootController // ControllerBase
    {
        private static Dictionary<string, Type> settingTypes = new Dictionary<string, Type>();
        /// <summary>
        /// add a type (originating in appsettings) that can be supplied by the SettingsController
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddSettingType<T>()
        {
            AddSettingType(typeof(T));
        }
        /// <summary>
        /// add a type (originating in appsettings) that can be supplied by the SettingsController
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public static void AddSettingType(string name, Type type)
        {
            if (!settingTypes.ContainsKey(name))
            {
                settingTypes.Add(name, type);
            }
        }
        /// <summary>
        /// add a type (originating in appsettings) that can be supplied by the SettingsController
        /// </summary>
        /// <param name="type"></param>
        public static void AddSettingType(Type type)
        {
            AddSettingType(type.Name, type);
        }
        private readonly IConfiguration configuration;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public SettingsController(IConfiguration config) //: base(logger)
        {
            configuration = config;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [HttpGet("get/type/{typeName}")]
        public object Get(string typeName)
        {
            if (settingTypes.ContainsKey(typeName))
            {
                Type t = settingTypes[typeName];
                var r = configuration.GetSection(typeName).Get(t);
                return Ok(r);
            }
            return NotFound();
        }

    }
}
