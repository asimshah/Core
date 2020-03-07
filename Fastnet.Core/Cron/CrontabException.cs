#region License and Terms
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Fastnet.Core.Cron
{
    #region Imports

    using System;
    using System.Runtime.Serialization;

    #endregion
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CrontabException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public CrontabException() :
            base("Crontab error.")
        { } // TODO: Fix message and add it to resource.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public CrontabException(string message) :
            base(message)
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CrontabException(string message, Exception innerException) :
            base(message, innerException)
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CrontabException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }
    }
}