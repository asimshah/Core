using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Fastnet.Core
{
    ///// <summary>
    ///// Compares strings using natural order for embedded numbers
    ///// </summary>
    //public class NaturalStringComparer : IComparer<string>
    //{
    //    private static Regex numbers = new Regex(@"\d+");
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    /// <returns></returns>
    //    public int Compare(string x, string y)
    //    {
    //        x = numbers.Replace(x, n => n.Value.PadLeft(8, '0'));
    //        y = numbers.Replace(y, n => n.Value.PadLeft(8, '0'));
    //        return x.CompareTo(y);
    //    }
    //}

    /// <summary>
    /// Compares strings using natural order for embedded numbers ignoring accents, punctuation and whitespace
    /// </summary>
    public class NaturalStringComparer : StringComparer// IComparer<string>
    {
        private static Regex numbers = new Regex(@"\d+");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override int Compare(string x, string y)
        {
            x = PadNumbers(x);
            y = PadNumbers(y);
            return string.Compare(x, y, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
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
        private string PadNumbers(string text)
        {
            return numbers.Replace(text, n => n.Value.PadLeft(8, '0'));
        }
    }
}
