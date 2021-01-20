using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultiTemplateGenerator.Lib
{
    /// <summary>
    /// Core Converter. Converts values generic dependent on data types.
    /// </summary>
    public static class Conversion
    {
        #region Numbers
        static readonly Regex regexNumbers = new Regex(@"^\d+$");
        public static bool IsNumericOnly(this string text)
        {
            //Use "^\d+$" if you need to match more than one digit.
            //Use "^[0-9]+$" to restrict matches to Arabic numerals.

            return regexNumbers.IsMatch(text);
        }

        public static bool ContainsNumeric(this string text)
        {
            foreach (char c in text)
            {
                if (char.IsNumber(c))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetNumbersOnly(this string value, bool includePeriod = false, bool includeSep = false)
        {
            string result = string.Empty;
            foreach (char c in value ?? "")
            {
                if (char.IsNumber(c) || includePeriod && c == '.' || includeSep && c == '-')
                {
                    result += c;
                }
            }

            if (includePeriod)
            {
                result = result.Trim('.');
            }

            return result;
        }

        #endregion
        public const string DefaultSeparator = ";";

        public static T ConvertTo<T>(this object value, T defaultValue = default(T))
        {
            if (value == null)
                return defaultValue;

            var result = GetValue(typeof(T), value);

            return (result == null)
                ? defaultValue
                : (T)result;
        }

        public static T GetValue<T>(object value, T defaultValue = default(T))
        {
            var result = GetValue(typeof(T), value);

            return (result == null)
                ? defaultValue
                : (T)result;
        }

        public static object GetValue(Type resultType, object value, string optional = null)
        {
            var isNull = value == null;
            if (!isNull)
            {
                //In case of a DBNull we need to do this
                isNull = value.ToString().Length == 0;
            }

            if (isNull)
            {
                if (resultType == typeof(string))
                    return string.Empty;

                return Activator.CreateInstance(resultType);
            }

            var valueType = value.GetType();
            if (resultType == valueType)
                return value;

            var seperator = !string.IsNullOrWhiteSpace(optional)
                ? optional
                : DefaultSeparator;

            //String list
            var list = value as IEnumerable;
            if (list != null && resultType == typeof(string))
            {
                var result = string.Empty;

                var dic = value as IDictionary<object, object>;
                if (dic != null)
                {
                    //Convert to readable 'Key=Value'
                    foreach (var item in dic)
                        result += string.Format("{0}={1}, ", item.Key.ToStringX(), item.Value.ToStringX());
                }
                else
                {
                    foreach (var item in list)
                        result += item.ToStringX().Trim() + seperator;
                }

                return result.Trim();
            }


            if (resultType == typeof(string))
                return value.ToString();
            if (value is string && (resultType == typeof(IEnumerable<string>) || resultType == typeof(ICollection<string>)))
            {
                return ((string)value).Split(seperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            if ((resultType == typeof(IList<string>) || resultType == typeof(List<string>)) && value is string)
            {
                return ((string)value).Split(seperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            if (resultType == typeof(DateTime) || resultType == typeof(DateTime?))
            {
                DateTime dtValue;
                if (DateTime.TryParse(value.ToString(), out dtValue))
                    return dtValue;
                return null;
            }
            if (resultType == typeof(TimeSpan) || resultType == typeof(TimeSpan?))
            {
                TimeSpan dtValue;
                if (TimeSpan.TryParse(value.ToString(), out dtValue))
                    return dtValue;
                return null;
            }
            if (resultType == typeof(bool) || resultType == typeof(bool?))
            {
                if (value.ToString().IsNumericOnly())
                {
                    value = Convert.ToInt32(value);
                    return (int)value > 0;
                }

                bool bValue;
                if (bool.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(decimal) || resultType == typeof(decimal?))
            {
                decimal bValue;
                if (decimal.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(double) || resultType == typeof(double?))
            {
                double bValue;
                if (double.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(float) || resultType == typeof(float?))
            {
                float bValue;
                if (float.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(short) || resultType == typeof(short?))
            {
                short bValue;
                if (short.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(int) || resultType == typeof(int?))
            {
                if (valueType == typeof(bool))
                {
                    return (bool)value ? 1 : 0;
                }
                int bValue;
                if (int.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(long) || resultType == typeof(long?))
            {
                long bValue;
                if (long.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(ulong) || resultType == typeof(ulong?))
            {
                ulong bValue;
                if (ulong.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }
            if (resultType == typeof(Guid) || resultType == typeof(Guid?))
            {
                Guid bValue;
                if (Guid.TryParse(value.ToString(), out bValue))
                    return bValue;
                return null;
            }


            throw new Exception("Data type not supported by ConverterHelper: " + resultType);
        }

        public static DateTime FromUnixDateTime(object unixDateTime)
        {
            return new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(unixDateTime));
        }

        ///// <returns></returns>
        ///// <param name="rawKey"></param>
        ///// </summary>
        /////     Converting Windown Product Key regiastry value

        ///// <summary>
        //[Obsolete("Flyttt denen")]
        //public static string DecryptProductKey(byte[] rawKey)
        //{
        //    try
        //    {
        //        var str1 = string.Empty;
        //        var num1 = 0;
        //        var num2 = checked(rawKey.Length - 1);
        //        var index1 = num1;
        //        while (index1 <= num2)
        //        {
        //            str1 = str1 + " " + rawKey[index1].ToString("X");
        //            checked
        //            {
        //                ++index1;
        //            }
        //        }
        //        var strArray1 = new string[25]
        //        {
        //            "B",
        //            "C",
        //            "D",
        //            "F",
        //            "G",
        //            "H",
        //            "J",
        //            "K",
        //            "M",
        //            "P",
        //            "Q",
        //            "R",
        //            "T",
        //            "V",
        //            "W",
        //            "X",
        //            "Y",
        //            "2",
        //            "3",
        //            "4",
        //            "6",
        //            "7",
        //            "8",
        //            "9",
        //            null
        //        };
        //        var num3 = 29;
        //        var num4 = 15;
        //        var strArray2 = new string[16];
        //        var str2 = "";
        //        var index2 = 52;
        //        do
        //        {
        //            strArray2[checked(index2 - 52)] = rawKey[index2].ToString();
        //            str2 = str2 + " " + Convert.ToInt32(strArray2[checked(index2 - 52)]).ToString("X");
        //            checked
        //            {
        //                ++index2;
        //            }
        //        } while (index2 <= 67);
        //        var expression = string.Empty;
        //        var num5 = checked(num3 - 1);
        //        while (num5 >= 0)
        //        {
        //            if (checked(num5 + 1)%6 == 0)
        //                expression = expression + "-";
        //            else
        //            {
        //                var index3 = 0;
        //                var index4 = checked(num4 - 1);
        //                while (index4 >= 0)
        //                {
        //                    var num6 = checked((int) Math.Round(unchecked(index3*256.0))) |
        //                               Convert.ToInt32(strArray2[index4]);
        //                    strArray2[index4] = Convert.ToString(num6/24);
        //                    index3 = num6%24;
        //                    checked
        //                    {
        //                        index4 += -1;
        //                    }
        //                }
        //                expression = expression + strArray1[index3];
        //            }
        //            checked
        //            {
        //                num5 += -1;
        //            }
        //        }
        //        return expression.Reverse();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("DigitalProductId value retrieved but error encountered whilst decrypting: " + ex.Message);
        //    }
        //}

        //UNIX time (32-bit value containing the number of seconds since 1/1/1970).        
    }
}
