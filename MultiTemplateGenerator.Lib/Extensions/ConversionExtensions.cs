using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace MultiTemplateGenerator.Lib
{
    public static class ConversionExtensions
    {
        public static void CopyPropertiesTo<TSource, TTarget>(this TSource source, TTarget target, IEnumerable<string> blackList)
        {
            var targetProperties = target.GetType().GetProperties();

            foreach (var propertyInfo in source.GetType().GetProperties().Where(x => !blackList.Contains(x.Name) && x.CanRead))
            {
                var targetProp = targetProperties.SingleOrDefault(x => x.Name.Equals(propertyInfo.Name) && x.CanWrite);
                if (targetProp == null)
                    continue;

                var propValue = propertyInfo.GetValue(source);
                targetProp.SetValue(target, propValue, null);
            }
        }

        public static void TrimProperties<T>(this T entity, bool setEmptyIfNull = true)
        {
            var entityList = entity as IList;
            if (entityList != null)
            {
                for (int index = 0; index < entityList.Count; index++)
                {
                    var item = entityList[index];
                    if (item is string)
                    {
                        item = ((string)item).Trim();
                        entityList[index] = item;
                    }
                    else
                        TrimProperties(item, setEmptyIfNull);
                }

                return;
            }

            foreach (var propertyInfo in entity.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    if (!propertyInfo.CanWrite)
                        continue;

                    var obj = propertyInfo.GetValue(entity, null);
                    if (obj != null)
                        propertyInfo.SetValue(entity, ((string)obj).Trim(), null);
                    else if (setEmptyIfNull)
                        propertyInfo.SetValue(entity, "", null);
                }
                else// if (propertyInfo.PropertyType.IsArray as IList != null)
                {
                    var list = propertyInfo.GetValue(entity, null) as IList;
                    if (list == null)
                        continue;
                    TrimProperties(list, setEmptyIfNull);
                }
            }
        }

        /// <summary>
        ///     Strips value if more than X digits.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this decimal value, int digits = 2)
        {
            try
            {
                var valueString = value.ToString();
                var pos = valueString.IndexOfAny(new[] { ',', '.' });
                if (pos != -1 && valueString.Length > pos + digits + 1)
                {
                    valueString = valueString.Substring(0, pos + digits + 1);
                    return decimal.Parse(valueString);
                }

                return value;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cannot convert decimal value '{0}' to " + digits + " digits: {1}", value, ex));
            }
        }

        public static int ToInt(this string text)
        {
            int res;
            int.TryParse(text, out res);
            return res;
        }

        public static short ToShort(this string text)
        {
            short res;
            short.TryParse(text, out res);
            return res;
        }

        public static string ToString(this object value)
        {
            if (value == null)
                return string.Empty;

            var list = value as IEnumerable;
            if (list != null)
            {
                var result = string.Empty;
                foreach (var item in list)
                {
                    result += item + "\r\n";
                }
                return result.TrimEnd();
            }
            return Convert.ToString(value);
        }

        public static string ToString<T>(this IList<T> list, string seperator = ",")
        {
            var result = string.Empty;

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(result))
                    result += seperator;

                result += item.ToString();
            }
            return result;
        }




        public static bool ToBool(this object text, bool defaultValue = false)
        {
            return text.ConvertTo(defaultValue);
        }

        public static bool? ToNullableBool(this object text, bool? defaultValue = null)
        {
            return text.ConvertTo(defaultValue);
        }

        public static string BoolToYesNo(this bool value)
        {
            return value ? "Yes" : "No";
        }

        public static string BoolToYesNo(this bool? value, string nullValue)
        {
            return !value.HasValue ? nullValue : value.Value ? "Yes" : "No";
        }

        public static double ToDouble(this object text, double defaultValue = 0)
        {
            return text.ConvertTo(defaultValue);
        }

        public static short ToInt16(this object text, short defaultValue = 0)
        {
            return text.ConvertTo(defaultValue);
        }

        public static int ToInt(this object text, int defaultValue = 0)
        {
            return text.ConvertTo(defaultValue);
        }

        public static List<int> ToInt(this IEnumerable<object> objects, int defaultValue = 0)
        {
            if (objects == null)
                return new List<int>(0);

            var list = objects.ToList();
            var result = new List<int>(list.Count);
            result.AddRange(list.Select(o => o.ToInt(defaultValue)));
            return result;
        }

        public static long ToInt64(this object text, long defaultValue = 0)
        {
            return text.ConvertTo(defaultValue);
        }

        public static List<long> ToInt64(this IEnumerable<object> objects, long defaultValue = 0)
        {
            if (objects == null)
                return new List<long>(0);

            var list = objects.ToList();
            var result = new List<long>(list.Count);
            result.AddRange(list.Select(o => o.ToInt64(defaultValue)));
            return result;
        }


        public static ulong ToUInt64(this object text, ulong defaultValue = 0)
        {
            return text.ConvertTo(defaultValue);
        }

        public static DateTime ToDateTime(this object value, DateTime defaultValue = new DateTime())
        {
            return value.ConvertTo(defaultValue);
        }

        public static DateTime? ToNullableDateTime(this object value, DateTime? defaultValue = null)
        {
            return value.ConvertTo(defaultValue);
        }

        public static bool IsBetween(this int value, int minValue, int maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static string ToStringX(this object value, string defaultValue = "")
        {
            if (value == null)
                return defaultValue;

            return value.ConvertTo(defaultValue);
        }

        public static string ToStringX<T>(this IList<T> list, string seperator = ",", int takeMax = 0)
        {
            var result = string.Empty;

            var count = 0;
            foreach (T item in list ?? new List<T>(0))
            {
                if (!string.IsNullOrEmpty(result))
                    result += seperator;

                result += item.ToString();

                count++;
                if (takeMax != 0 && count == takeMax)
                    break;
            }
            return result;
        }

        public static string ToStringX<T>(this IList<T> list, char seperator, int takeMax = 0)
        {
            return ToStringX(list, seperator.ToString(), takeMax);
        }

        public static string ToHex(this int value)
        {
            return $"0x{value:X}";
        }

        public static byte[] ToArray(this Stream stream)
        {
            if (stream == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }



        #region Dictionary

        public static int ToInt(this IDictionary<string, object> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var result)
                ? Convert.ToInt32(result)
                : 0;
        }

        public static string ToSeparatedString(this IDictionary<string, string> dictionary, string separator)
        {
            if (dictionary == null)
                return string.Empty;

            var result = string.Empty;

            foreach (var val in dictionary)
                result += val.Value + separator;

            if (!string.IsNullOrEmpty(result))
                result = result.Substring(0, result.Length - separator.Length);

            return result;
        }

        public static List<string> ToListX(this NameValueCollection collection, string divider)
        {
            var result = new List<string>(collection?.Count ?? 0);
            if (collection == null)
                return result;

            foreach (var name in collection.Keys)
                result.Add(name + divider + collection.Get(name.ToString()));

            return result;
        }

        public static object GetValue(this IDictionary<string, object> dictionary, string key)
        {
            return !dictionary.ContainsKey(key)
                ? null
                : dictionary[key];
        }

        public static string GetString(this Dictionary<string, object> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var result)
                ? result.ToString()
                : null;
        }

        #endregion
    }
}
