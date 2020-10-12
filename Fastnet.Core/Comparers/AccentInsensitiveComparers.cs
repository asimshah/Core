using System;
using System.Collections.Generic;
using System.Globalization;

namespace Fastnet.Core
{
    ///// <summary>
    ///// an equality comparer that uses a function/lambda to test equality
    ///// in many case this avoids creating a custom IEqualityComparer
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //[Obsolete("does not work if the equalsFunction alters the data in some way as then the hashcode is incorrect - abandoned")]
    //public class LambdaEqualityComparer<T> : IEqualityComparer<T> 
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="equalsFunction"></param>
    //    public LambdaEqualityComparer(Func<T, T, bool> equalsFunction)
    //    {
    //        _equalsFunction = equalsFunction;
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    /// <returns></returns>
    //    public bool Equals(T x, T y)
    //    {
    //        return _equalsFunction(x, y);
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="obj"></param>
    //    /// <returns></returns>
    //    public int GetHashCode(T obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //    private readonly Func<T, T, bool> _equalsFunction;
    //}
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
        private readonly bool ignoreWhitespace;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignoreSpaces"></param>
        public AccentAndCaseInsensitiveComparer(bool ignoreSpaces = false)
        {
            ignoreWhitespace = ignoreSpaces;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override int Compare(string x, string y)
        {
            if (ignoreWhitespace)
            {
                return string.Compare(x.Replace(" ", string.Empty), y.Replace(" ", string.Empty), CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
            }
            else
            {
                return string.Compare(x, y, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
            }
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
            if (ignoreWhitespace)
            {
                return obj.Replace(" ", string.Empty).RemoveDiacritics().GetHashCode();
            }
            else
            {
                return obj.RemoveDiacritics().GetHashCode();
            }               
        }
    }
}
