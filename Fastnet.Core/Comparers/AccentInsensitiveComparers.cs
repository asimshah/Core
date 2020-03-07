using System;
using System.Globalization;

namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class AccentInsensitiveComparer : StringComparer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override int Compare(string x, string y)
        {
            return string.Compare(x, y, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Equals(string x, string y)
        {
            return Compare(x, y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int GetHashCode(string obj)
        {
            return obj.RemoveDiacritics().GetHashCode();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class AccentAndCaseInsensitiveComparer : StringComparer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override int Compare(string x, string y)
        {
            return string.Compare(x, y, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Equals(string x, string y)
        {
            return Compare(x, y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override int GetHashCode(string obj)
        {
            return obj.RemoveDiacritics().GetHashCode();
        }
    }
}
