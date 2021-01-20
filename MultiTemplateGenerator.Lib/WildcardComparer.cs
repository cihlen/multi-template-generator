using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xaml;

namespace MultiTemplateGenerator.Lib
{
    public class WildcardComparer : IEqualityComparer<string>
    {
        // If you want to implement "*" only
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        public bool Equals(string x, string y)
        {
            if (x == null || y == null) return false;

            if (x.Equals(y, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (x.IndexOf('*') != -1)
            {
                return Regex.IsMatch(y, WildCardToRegular(x));
            }
            if (y.IndexOf('*') != -1)
            {
                return Regex.IsMatch(x, WildCardToRegular(y));
            }
            
            return false;
        }

        public int GetHashCode(string obj)
        {
            throw new NotImplementedException();
        }
    }
}
