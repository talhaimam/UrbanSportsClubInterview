using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;

namespace InterviewService.Client.Extensions
{
    public static class ToQueryStringExtension
    {
        public static string ToQueryString(this object obj)
        {
            List<string> queryItems = new List<string>();

            foreach (var property in obj.GetType().GetProperties())
            {
                string name = property.Name.ToCamelCase();
                object value = property.GetValue(obj);

                if (value != null)
                {
                    if (value is Array)
                    {
                        foreach (object item in (Array)value)
                        {
                            queryItems.Add(name + "=" + value.ToValueString());
                        }
                    }
                    else
                    {
                        queryItems.Add(name + "=" + value.ToValueString());
                    }
                }
            }

            return "?" + string.Join("&", queryItems);
        }

        private static string ToCamelCase(this string str)
        {
            switch (str)
            {
                case null: throw new ArgumentNullException(nameof(str));
                case "": throw new ArgumentException($"{nameof(str)} cannot be empty", nameof(str));
                default: return str[0].ToString().ToLower() + str.Substring(1);
            }
        }

        private static string ToValueString(this object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string)
            {
                return (string)value;
            }

            if (value is bool)
            {
                return value.ToString();
            }

            if (value is int)
            {
                return value.ToString();
            }

            if (value is float)
            {
                return value.ToString();
            }

            if (value is double)
            {
                return value.ToString();
            }

            if (value is Guid)
            {
                return value.ToString();
            }

            if (value is Enum)
            {
                return value.ToString();
            }

            if (value is TimeSpan)
            {
                return value.ToString();
            }

            if (value is DateTime)
            {
                return HttpUtility.UrlEncode(JsonConvert.SerializeObject(value).Trim('"'));
            }

            if (value is DateTimeOffset)
            {
                return HttpUtility.UrlEncode(JsonConvert.SerializeObject(value).Trim('"'));
            }

            throw new NotImplementedException();
        }
    }
}