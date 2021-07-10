using Fastnet.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fastnet.Core
{
    /// <summary>
    /// 
    /// </summary>
    public enum Browsers
    {
        /// <summary>
        /// 
        /// </summary>
        Unknown,
        /// <summary>
        /// 
        /// </summary>
        Edge,
        /// <summary>
        /// 
        /// </summary>
        IE,
        /// <summary>
        /// 
        /// </summary>
        Chrome,
        /// <summary>
        /// 
        /// </summary>
        Firefox,
        /// <summary>
        /// 
        /// </summary>
        Safari
    }
    /// <summary>
    /// 
    /// </summary>
    public class AssemblyVersion
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PackageVersion { get; set; }
    }
    /// <summary>
    /// warning: careful if you use these extensions!!
    /// Do I really need these methods????
    /// </summary>
    public static class uk_time_extensions
    {
        // this runs at static contsructor time and wil lnot work on all hosts: e.g does not work in Blazor Webassembly!
        private static readonly TimeZoneInfo ukTime = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        /// <summary>
        /// Converts the date (assumed to be UTC) to UK time and returns the default format (ddMMMyyyy)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToUKDefault(this DateTime d)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(d, ukTime).ToDefault();
        }
        /// <summary>
        /// Converts the date to UK time and returns the default format (ddMMMyyyy)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToUKDefault(this DateTimeOffset d)
        {
            return TimeZoneInfo.ConvertTime(d, ukTime).ToDefault();
        }
        /// <summary>
        /// Converts the date (assumed to be UTC) to UK time and returns the default format with time (ddMMMyyyy HH:mm:ss)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToUKDefaultWithTime(this DateTime d)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(d, ukTime).ToDefaultWithTime();
        }
        /// <summary>
        /// Converts the date to UK time and returns the default format with time (ddMMMyyyy HH:mm:ss)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToUKDefaultWithTime(this DateTimeOffset d)
        {
            return TimeZoneInfo.ConvertTime(d, ukTime).ToDefaultWithTime();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        //regex from http://detectmobilebrowsers.com/
        private static readonly Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly ILogger log = ApplicationLoggerFactory.CreateLogger("Fastnet.Core.Extensions");
        private static readonly Regex splitToWords = new Regex(@"(\b[^\s]+\b)", RegexOptions.IgnoreCase);
        private static readonly Regex removeNonAlphaNumerics = new Regex(@"[^a-zA-Z0-9\p{L}]", RegexOptions.IgnoreCase);

        /// <summary>
        /// get the last 'word' in the string using a single space separator
        /// (roman numerals at the end are ignored)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetLastName(this string text)
        {
            var parts = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 2)
            {
                switch (parts.Last().ToLower())
                {
                    case "i":
                    case "ii":
                    case "iii":
                    case "iv":
                    case "v":
                    case "vi":
                    case "vii":
                    case "viii":
                    case "ix":
                        parts = parts.Take(parts.Length - 1).ToArray();
                        break;
                }
            }
            return parts.Length > 1 ? parts.Last() : parts.First();
        }
        /// <summary>
        /// checks if the given ip address is in the range of a cidr address
        /// </summary>
        /// <param name="stringAddress">like 192.168.0.23</param>
        /// <param name="cidrAddress">like 192.168.0.0/24</param>
        /// <returns></returns>
        public static bool IsInIPRange(this string stringAddress, string cidrAddress)
        {
            return IsInIPRange(cidrAddress.ToIPNetwork(), stringAddress.ToIPNetwork());
        }
        /// <summary>
        /// checks if the given ip address is in the range of a cidr address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cidrAddress">like 192.168.0.0/24</param>
        /// <returns></returns>
        public static bool IsInIPRange(this IPNetwork address, string cidrAddress)
        {
            return IsInIPRange(cidrAddress.ToIPNetwork(), address);
        }
        /// <summary>
        /// checks if the given ip address is in the range of a cidr address
        /// </summary>
        /// <param name="stringAddress"></param>
        /// <param name="cidrAddress"></param>
        /// <returns></returns>
        public static bool IsInIPRange(this string stringAddress, IPNetwork cidrAddress)
        {
            return IsInIPRange(stringAddress.ToIPNetwork(), cidrAddress);
        }
        /// <summary>
        /// checks if the given ip address is in the range of a cidr address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cidrAddress"></param>
        /// <returns></returns>
        public static bool IsInIPRange(this IPNetwork address, IPNetwork cidrAddress)
        {
            return cidrAddress.Contains(address);
            //return IPNetwork.Overlap(cidrAddress, address);
        }
        /// <summary>
        /// converts an string ip address to an instance of IPNetwork
        /// </summary>
        /// <param name="stringAddress"></param>
        /// <returns></returns>
        public static IPNetwork ToIPNetwork(this string stringAddress)
        {
            return IPNetwork.Parse(stringAddress);
        }
        /// <summary>
        /// converts an IPAddress to an instance of IPNetwork
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static IPNetwork ToIPNetwork(this IPAddress address)
        {
            return address.ToString().ToIPNetwork();
        }
        /// <summary>
        /// Convert an object into a json style string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="indented">true to indent output, default = false</param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj, bool indented = false, JsonSerializerSettings settings = null)
        {
            //return JsonConvert.SerializeObject(obj, settings);
            return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None, settings);
        }
        /// <summary>
        /// convert json to an instance of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T ToInstance<T>(this string jsonString, JsonSerializerSettings settings = null)
        {
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                return JsonConvert.DeserializeObject<T>(jsonString, settings);
            }
            return default;
        }
        /// <summary>
        /// convert json to an instance of the requested Type
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static object ToInstance(this string jsonString, Type type, JsonSerializerSettings settings = null)
        {
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                return JsonConvert.DeserializeObject(jsonString, type, settings);
            }
            return null;
        }
        /// <summary>
        /// returns a string with all diacritics (accents) removed
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string s)
        {
            var normalised = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            for (int i = 0; i < normalised.Length; i++)
            {
                Char c = normalised[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        /// <summary>
        /// returns text with invalid path name chars removed
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetPathSafeString(this string text)
        {
            text = text.Replace("\"", "'");
            return string.Join("", text.Split(Path.GetInvalidFileNameChars()));
        }
        /// <summary>
        /// Compares two string ignoring accents and case
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int CompareIgnoreAccentsAndCase(this string left, string right)
        {
            return string.Compare(left, right,
                CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
        }
        /// <summary>
        /// Indicates whether two strings are equal ignoring case
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool IsEqual(this string left, string right)
        {
            return String.Compare(left, right, true) == 0;
        }
        /// <summary>
        /// Indicates whether two strings are equal ignoring both case and accents
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool IsEqualIgnoreAccentsAndCase(this string left, string right)
        {
            //return string.Compare(left, right,
            //    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols) == 0;
            return left.CompareIgnoreAccentsAndCase(right) == 0;
        }
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string
        /// in the CurrentCulture and with given compare options
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool StartsWith(this string str, string value, CompareOptions options)
        {
            return str.StartsWith(value, CultureInfo.CurrentCulture, options);
        }
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string
        /// in the given culture and with given compare options
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="culture"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool StartsWith(this string str, string value, CultureInfo culture, CompareOptions options)
        {
            return culture.CompareInfo.IsPrefix(str, value, options);
        }
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string
        /// ignoring both case and accents
        /// </summary>
        /// <param name="src"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool StartsWithIgnoreAccentsAndCase(this string src, string text)
        {
            return src?.StartsWith(text, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols) ?? false;
        }
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string
        /// in the CurrentCulture and with given compare options
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool EndsWith(this string str, string value, CompareOptions options)
        {
            return str.EndsWith(value, CultureInfo.CurrentCulture, options);
        }
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string
        /// in the given culture and with given compare options
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="culture"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool EndsWith(this string str, string value, CultureInfo culture, CompareOptions options)
        {
            return culture.CompareInfo.IsSuffix(str, value, options);
        }
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string
        /// ignoring both case and accents
        /// </summary>
        /// <param name="src"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool EndsWithIgnoreAccentsAndCase(this string src, string text)
        {
            return src?.EndsWith(text, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols) ?? false;
        }
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string
        /// </summary>
        /// <param name="string"></param>
        /// <param name="substring"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static bool Contains(this string @string, string substring, StringComparison comparisonType)
        {
            if (@string == null || substring == null)
            {
                return false;
            }
            return @string?.IndexOf(substring, comparisonType) >= 0;
        }
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string
        /// </summary>
        /// <param name="string"></param>
        /// <param name="substring"></param>
        /// <param name="culture"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Contains(this string @string, string substring, CultureInfo culture, CompareOptions options)
        {
            return culture.CompareInfo.IndexOf(@string, substring, options) >= 0;
        }
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string ignoring case and accents
        /// </summary>
        /// <param name="string"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool ContainsIgnoreAccentsAndCase(this string @string, string substring)
        {
            return @string?.Contains(substring, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols) ?? false;
        }
        internal static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var typeInfo = enumVal.GetType().GetTypeInfo();
            var v = typeInfo.DeclaredMembers.First(x => x.Name == enumVal.ToString());
            return v.GetCustomAttribute<T>();
        }
        /// <summary>
        /// Gets the text of the description attribute on an enum value
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="val">Enum value</param>
        /// <returns></returns>
        public static string ToDescription<T>(this T val) where T : struct
        {
            if (typeof(T).GetTypeInfo().IsEnum)
            {
                var list = (DescriptionAttribute[])typeof(T).GetTypeInfo().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                return list?.Length > 0 ? list.First().Description : val.ToString();
            }
            throw new ArgumentException("Not an enum type");
        }
        /// <summary>
        /// Convert string to enum of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static T To<T>(this string text) where T : struct
        {
            if (typeof(T).GetTypeInfo().IsEnum)
            {
                var values = Enum.GetNames(typeof(T));
                if (values.Contains(text, StringComparer.CurrentCultureIgnoreCase)) //.InvariantCultureIgnoreCase))
                {
                    return (T)Enum.Parse(typeof(T), text, true);
                }
                else
                {
                    foreach (var s in values)
                    {
                        var list = (DescriptionAttribute[])typeof(T).GetTypeInfo().GetField(s).GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (list.Length > 0)
                        {
                            if (string.Compare(list[0].Description, text, true) == 0)
                            {
                                return (T)Enum.Parse(typeof(T), s, true);
                            }
                        }
                    }
                }
                throw new ArgumentOutOfRangeException($"{text} not valid");
            }
            throw new ArgumentException("Not an enum type");
        }
        /// <summary>
        /// Returns the text having removed all chars except alphabetics and digits
        /// (accents are also removed)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToAlphaNumerics(this string text)
        {
            text = text.RemoveDiacritics();
            return removeNonAlphaNumerics.Replace(text, string.Empty);
        }
        /// <summary>
        /// create a CSV string from a list of strings
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string ToCSV(this IEnumerable<string> strings)
        {
            strings ??= new string[0];
            return string.Join(", ", strings);
        }
        /// <summary>
        /// create a CSV string from any type using a string selector lambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string ToCSV<T>(this IEnumerable<T> list, Func<T,string> selector)
        {
            return list.Select(x => selector(x)).ToCSV();
        }
        /// <summary>
        /// Splits text into an array of words
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] ToWords(this string text)
        {
            return splitToWords.Split(text);
        }
        /// <summary>
        /// Convert a number into Roman
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToRoman(this int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("Value must be between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900); //EDIT: i've typed 400 instead 900
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("Value must be between 1 and 3999");
        }
        /// <summary>
        /// returns timespan as hrs and minutes
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string ToDefault(this TimeSpan ts)
        {
            var hrs = (ts.Days * 24) + ts.Hours;
            if (hrs > 0)
            {
                return $"{hrs} hrs {ts.Minutes} mins";
            }
            else
            {
                return $"{ts.Minutes} mins {ts.Seconds} secs";
            }
        }
        /// <summary>
        /// Returns the date in the default string format (ddMMMyyyy)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDefault(this DateTime d)
        {
            return d.ToString("ddMMMyyyy");
        }
        /// <summary>
        /// Returns the date in the default string format (ddMMMyyyy)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDefault(this DateTimeOffset d)
        {
            return d.ToString("ddMMMyyyy");
        }
        /// <summary>
        /// Returns the date in the default string format with the time (ddMMMyyyy HH:mm:ss)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDefaultWithTime(this DateTime d)
        {
            return d.ToString("ddMMMyyyy HH:mm:ss");
        }
        /// <summary>
        /// Returns the date in the default string format with the time (ddMMMyyyy HH:mm:ss)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDefaultWithTime(this DateTimeOffset d)
        {
            var fmt = "ddMMMyyyy HH:mm:ss";
            return d.ToString(fmt);
        }

        /// <summary>
        /// Rounds date up by minutes
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static DateTime RoundUp(this DateTime dt, int m)
        {
            var od = dt;
            DateTime rd;
            if (m > 60)
            {
                od = dt.AddMinutes(-dt.Minute).AddSeconds(-dt.Second);
                var h = (int)Math.Round((double)m / 60);
                var ts = TimeSpan.FromHours(od.Hour);
                var target = TimeSpan.FromHours(h * Math.Ceiling((ts.TotalHours + 0.5) / h));
                var x = target.TotalHours - ts.TotalHours;
                rd = od.AddHours(x);
            }
            else
            {
                int minutes = Math.Min(60, m);
                var ts = TimeSpan.FromMinutes(od.Minute);
                //var target = ts.RoundUp(minutes);
                var target = TimeSpan.FromMinutes(minutes * Math.Ceiling((ts.TotalMinutes + 0.5) / minutes));
                var x = target.TotalMinutes - ts.TotalMinutes;
                rd = od.AddMinutes(x).AddSeconds(-od.Second);
            }
            return rd;
        }
        /// <summary>
        /// Returns a new instance for the same date but a time of 00:00:00 and a zero timespan offset
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTimeOffset StripTimeAndZone(this DateTimeOffset dt)
        {
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, TimeSpan.Zero);
        }
        /// <summary>
        /// Checks if a folder can be accessed
        /// </summary>
        /// <param name="folderName">full name of folder to access</param>
        /// <param name="CanWrite">checks that specified folder can be written to by writing a probe file</param>
        /// <param name="createIfRequired">creates specified folder if not present</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static bool CanAccess(this string folderName, bool CanWrite = false, bool createIfRequired = false, ILogger log = null)
        {
            try
            {
                if (createIfRequired && !Directory.Exists(folderName))
                {
                    try
                    {
                        Directory.CreateDirectory(folderName);
                    }
                    catch (Exception)
                    {
                        log?.Error($"cannot create {folderName}, current Environment.UserName = {Environment.UserName}");
                        throw;
                    }
                }
                var contentCount = Directory.EnumerateFileSystemEntries(folderName).Count();
                if (CanWrite)
                {
                    var probe = Path.Combine(folderName, $"${Guid.NewGuid().ToString()}.probe.txt");
                    try
                    {
                        //var probe = Path.Combine(folderName, "pr$obe.txt");
                        File.WriteAllText(probe, "test write");
                        File.Delete(probe);
                    }
                    catch (Exception)
                    {
                        log?.Error($"cannot write probe file to {folderName}, current Environment.UserName = {Environment.UserName}");
                        throw;
                    }
                    finally
                    {
                        if (File.Exists(probe))
                        {
                            File.Delete(probe);
                        }
                    }
                }
                return true;
            }
            catch (Exception xe)
            {
                log?.Error(xe, $"current Environment.UserName = {Environment.UserName}");
                return false;
            }
        }
        /// <summary>
        /// logs a debug message including caller member and line number
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="text"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Debug(this ILogger logger, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogDebug($"{text}, @line .{name}-{line}");
        }
        /// <summary>
        /// logs a trace message including caller member and line number
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="text"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Trace(this ILogger logger, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogTrace($"{text}, @line .{name}-{line}");
        }
        /// <summary>
        /// logs a warning message including caller member and line number
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="text"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Warning(this ILogger logger, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogWarning($"{text}, @line .{name}-{line}");
        }
        /// <summary>
        /// logs a information message including caller member and line number
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="text"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Information(this ILogger logger, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogInformation($"{text}, @line .{name}-{line}");
        }
        /// <summary>
        /// logs an error message including caller member and line number
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Error(this ILogger logger, string message, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogError($"{message}, @line .{name}-{line}");
        }
        /// <summary>
        /// logs an exception plus message and includes caller member, line number, stacjk trace and inner exceptions
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="xe"></param>
        /// <param name="message"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Error(this ILogger logger, Exception xe, string message, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogError($"{message}, {xe.Message}\n{InsertExtendedTrace(xe.StackTrace, xe.InnerException)}, @line .{name}-{line}");
        }
        /// <summary>
        /// logs an exception and includes caller member, line number, stacjk trace and inner exceptions
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="xe"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        public static void Error(this ILogger logger, Exception xe, [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        {
            logger.LogError($"{xe.Message}\n{InsertExtendedTrace(xe.StackTrace, xe.InnerException)}, @line .{name}-{line}");
        }
        /// <summary>
        /// Serialise all inner exceptions to a string
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        public static string AllToString(this Exception xe)
        {
            var msgs = new List<string>();
            do
            {
                msgs.Add(xe.Message);
                xe = xe.InnerException;
            } while (xe != null);
            msgs.Reverse();
            return string.Join(" <= ", msgs);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        public static IEnumerable<AssemblyVersion> GetVersions(this Assembly executingAssembly)
        {
            static AssemblyVersion getAssemblyVersion(AssemblyName assemblyName)
            {
                var assembly = Assembly.Load(assemblyName);
                var productVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion; 
                Version assemblyVersion = assembly.GetName().Version;
                // version
                // .major = version number
                // .minor = yyMM, year and month
                // .build = ddHH, day in month and hours (0 - 24)
                // .minor = mmSS, minues and seconds
                var year = 2000 + assemblyVersion.Minor / 100; var month = assemblyVersion.Minor % 100;
                var day = assemblyVersion.Build / 100; var hour = assemblyVersion.Build % 100;
                var minutes = assemblyVersion.Revision / 100; var seconds = assemblyVersion.Revision % 100;
                var vd = new DateTime(year, month, day, hour, minutes, seconds);
                return new AssemblyVersion { Name = assemblyName.Name, Version = assemblyVersion.ToString(), DateTime = vd, PackageVersion = productVersion };
            }
            var list = new List<AssemblyVersion>();
            list.Add(getAssemblyVersion(executingAssembly.GetName()));
            //var assemblyLocation = executingAssembly.Location;
            ////executingAssembly.GetName();
            ////productVersion is v.days.minutes, days = days since 1Jan2020, minutes since midnight 
            //var productVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
            //var parts = productVersion.Split(".");
            //if(parts.Length == 3)
            //{
            //    var days = int.Parse(parts[1]);
            //    var dt = DateTime.Parse("2020-01-01").AddDays(days);
            //    var minutes = int.Parse(parts[2]);
            //    dt = dt.AddMinutes(minutes);
            //    list.Add(new AssemblyVersion {  Name  = executingAssembly.GetName().Name, Version = productVersion, DateTime = dt});
            //}

            foreach (var assemblyName in executingAssembly.GetReferencedAssemblies())
            {
                if (assemblyName.Name.StartsWith("fastnet", System.Globalization.CompareOptions.IgnoreCase))
                {
                    if (!assemblyName.Name.EndsWith("private", System.Globalization.CompareOptions.IgnoreCase)) {
                        var v = getAssemblyVersion(assemblyName);
                        list.Add(v);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// Get the package version from the assembly (using location and ProductVersion)
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        [Obsolete]
        public static string GetPackageVersion(this Assembly assembly)
        {            
            var assemblyLocation = assembly.Location;
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
            var text = $"{version}";
            var parts = version.Split('.');
            if (parts.Length == 4)
            {
                try
                {
                    var t = int.Parse(parts[1]);
                    var year = 2000 + t / 100; var month = t % 100;
                    t = int.Parse(parts[2]);
                    var day = t / 100; var hour = t % 100;
                    t = int.Parse(parts[3]);
                    var minutes = t / 100; var seconds = t % 100;
                    var vd = new DateTime(year, month, day, hour, minutes, seconds);
                    text = $"{version} [{vd.ToDefaultWithTime()}]";
                }
                catch {  }
            }
            return text;
        }
        /// <summary>
        /// get the assembly version using the assembly Version property
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyVersion(this Assembly assembly)
        {
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// workaround for multiple OnChange calls when a monitored options value changes
        /// (this is due to a problem with the way that file change notifications work - will Microsoft fix this?)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionsMonitor"></param>
        /// <param name="action"></param>
        /// <param name="delay">in milliseconds, default value is 2000</param>
        public static void OnChangeWithDelay<T>(this IOptionsMonitor<T> optionsMonitor, Action<T> action, int delay = 2000)
        {
            bool changeDelayStarted = false;
            optionsMonitor.OnChange(async (opt) =>
            {
                if (!changeDelayStarted)
                {
                    changeDelayStarted = true;
                    await Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(delay));
                        action(opt);
                        changeDelayStarted = false;
                    });
                }
                //Debug.WriteLine("OnChangeWithDelay");
            });
        }
        /// <summary>
        /// Allows fastnet.Core services to be configured - only necessary at present for messenger multicast autoconfigure
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureCoreServices(this IServiceCollection services)
        {
            services.ConfigureMessengerOptions();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static bool IsMobileBrowser(this string userAgent)
        {
            //var userAgent = request.UserAgent();
            if (userAgent != null && ((b.IsMatch(userAgent) || v.IsMatch(userAgent.Substring(0, 4)))))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static Browsers ParseUserAgentForBrowser(this string userAgent)
        {
            Browsers browser = Browsers.Unknown;
            if (userAgent != null)
            {
                var text = userAgent.ToLower();
                //if (text.Contains("edge"))
                if (text.Contains("edg")) // found this in at least one case
                {
                    browser = Browsers.Edge;
                }
                else if (text.Contains("chrome"))
                {
                    browser = Browsers.Chrome;
                }
                else if (text.Contains("safari"))
                {
                    browser = Browsers.Safari;
                }
                else if (text.Contains("firefox"))
                {
                    browser = Browsers.Firefox;
                }
                else if (text.Contains("msie"))
                {
                    browser = Browsers.IE;
                }
                else if (text.Contains("iemobile"))
                {
                    browser = Browsers.IE;
                }
            }
            return browser;
        }
        /// <summary>
        /// returns the text hashed using a one-way algorithm
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Hash(this string text)
        {
            return SecurePasswordHasher.Hash(text);
        }
        /// <summary>
        /// returns the text hashed using a one-way algorithm
        /// </summary>
        /// <param name="text"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static string Hash(this string text, int iterations)
        {
            return SecurePasswordHasher.Hash(text, iterations);
        }
        /// <summary>
        /// Verifies text against its previously hashed version
        /// </summary>
        /// <param name="text"></param>
        /// <param name="hashedText"></param>
        /// <returns></returns>
        public static bool Verify(this string text, string hashedText)
        {
            return SecurePasswordHasher.Verify(text, hashedText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="passPhrase"></param>
        /// <returns>a json string that can be decrypted using the same passPhrase</returns>
        public static string Encrypt(this string plainText, string passPhrase)
        {
            return EncryptionHandler.Encrypt(plainText, passPhrase);
        }
        /// <summary>
        /// 
        /// </summary>

        public static string Decrypt(this string cipherText, string passPhrase)
        {
            return EncryptionHandler.Decrypt(cipherText, passPhrase);
        }
        private static string InsertExtendedTrace(string stackTrace, Exception innerException)
        {
            var sb = new StringBuilder(stackTrace);
            foreach (var msg in InnerExceptions(innerException))
            {
                sb.AppendLine($"   {msg}");
            }
            return sb.ToString();
        }
        private static IEnumerable<string> InnerExceptions(Exception inner)
        {
            var list = new List<string>();
            while (inner != null)
            {
                list.Add($"{inner.Message}");
                inner = inner.InnerException;
            }
            return list;
            //var sb = new StringBuilder();
            //while (inner != null)
            //{
            //    sb.AppendLine($"-->{inner.Message}");
            //    inner = inner.InnerException;
            //}
            //return sb.ToString();
        }
    }


}
