﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI;

namespace AppFramework.Core
{
    public static partial class GeneralFunctions
    {
        /// <summary>
        /// Converts a string hex color value to a Windows.UI.Color object instance.
        /// </summary>
        /// <param name="hexColor">Hex value of the color.</param>
        /// <returns>Windows.UI.Color instance for the specified hex color value.</returns>
        public static Color HexToColor(string hexColor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hexColor))
                    return Colors.Transparent;

                // Remove # if present
                if (hexColor.IndexOf('#') != -1)
                    hexColor = hexColor.Replace("#", "");

                hexColor = hexColor.ToLower();

                int red = 0;
                int green = 0;
                int blue = 0;

                if (hexColor.Length == 6)
                {
                    // #RRGGBB
                    red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                    green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                    blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (hexColor.Length == 3)
                {
                    // #RGB
                    red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                    green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                    blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                }

                return Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Could not convert hex value '{0}' to a Color.", hexColor), ex);
            }
        }

        /// <summary>
        /// Parses a query string into a dictionary of key/value pairs of each parameter in the string.
        /// </summary>
        /// <param name="querystring">Url or querystring string to parse.</param>
        /// <returns>Dictionary of key value pairs found within the query string.</returns>
        public static IDictionary<string, string> ParseQuerystring(string querystring)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(querystring))
            {
                if (Uri.IsWellFormedUriString(querystring, UriKind.Absolute))
                    querystring = new Uri(querystring).Query;
                WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(querystring);
                foreach (var entry in decoder)
                    dic.Add(entry.Name, entry.Value);
            }
            return dic;
        }

        /// <summary>
        /// Converts a dictionary of key/value pairs into a query string.
        /// </summary>
        /// <param name="parameters">Key/value pairs of parameters.</param>
        /// <returns>Query string from all the key/value pair data supplied in the dictionary.</returns>
        public static string CreateQuerystring(IDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return null;
            else
            {
                var contents = new Windows.Web.Http.HttpFormUrlEncodedContent(parameters);
                return contents.ReadAsStringAsync().AsTask().Result;
            }
        }

        /// <summary>
        /// Creates a dictionary object with an initial key/value pair entry.
        /// </summary>
        /// <typeparam name="T">Type of the key</typeparam>
        /// <typeparam name="S">Type of the value</typeparam>
        /// <param name="key">Value representing the unique key</param>
        /// <param name="value">Value belonging to the key</param>
        /// <returns>Dictionary object instance.</returns>
        internal static Dictionary<T, S> CreateDictionary<T, S>(T key, S value)
        {
            var dic = new Dictionary<T, S>();
            dic.Add(key, value);
            return dic;
        }

        internal static string ConvertPhoneLettersToNumbers(string phone)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    phone = phone.ToUpper();
                    phone = Regex.Replace(phone, "[abcABC]", "2");
                    phone = Regex.Replace(phone, "[defDEF]", "3");
                    phone = Regex.Replace(phone, "[ghiGHI]", "4");
                    phone = Regex.Replace(phone, "[jklJKL]", "5");
                    phone = Regex.Replace(phone, "[mnoMNO]", "6");
                    phone = Regex.Replace(phone, "[pqrsPQRS]", "7");
                    phone = Regex.Replace(phone, "[tuvTUV]", "8");
                    phone = Regex.Replace(phone, "[wxyzWXYZ]", "9");
                    phone = Regex.Replace(phone, "[)]", "-");
                    phone = Regex.Replace(phone, "[( ]", "");
                    phone = Regex.Replace(phone, "[.]", "");
                    return phone.Trim();
                }
            }
            catch
            {
            }

            return phone;
        }
    }
}
