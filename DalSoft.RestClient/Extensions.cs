﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DalSoft.RestClient
{
    internal static class Extensions
    {
        public static bool IsHttpVerb(string httpMethod)
        {
            return typeof(HttpMethod).GetProperties().Any(x => x.Name == httpMethod);
        }

        public static bool IsImmutableVerb(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpper();

            return httpMethod == HttpMethodEnum.GET.ToString() || httpMethod == HttpMethodEnum.DELETE.ToString() ||
                   httpMethod == HttpMethodEnum.HEAD.ToString() || httpMethod == HttpMethodEnum.OPTIONS.ToString() ||
                   httpMethod == HttpMethodEnum.TRACE.ToString();
        }

        public static bool IsMutableVerb(this string httpMethod)
        {
            httpMethod = httpMethod.ToUpper();
            return httpMethod == HttpMethodEnum.POST.ToString() || httpMethod == HttpMethodEnum.PUT.ToString();
        }

        public static string GetUri(string currentUri, object[] args)
        {
            if (args.Length > 0 && args[0] != null && args[0].GetType().IsPrimitive)
                currentUri += args[0].ToString();

            if (currentUri.EndsWith("/"))
                currentUri = currentUri.TrimEnd("/".ToCharArray());

            return currentUri;
        }

        public static void ParseHttpVerbArgs(this object[] args)
        {
            if (args.Length == 0)
                return;

            if (args.Length > 2)
                throw new ArgumentException("You can only pass two arguments, first is the resource or object to be serialized, second is the RequestHeaders");

            if (args.Length == 2 && (args[1] as IDictionary<string, string>) == null)
                throw new ArgumentException("Second argument must be RequestHeaders");
        }

        public static IDictionary<string, string> GetRequestHeaders(this object[] args)
        {
            IDictionary<string, string> requestHeaders = new Dictionary<string, string> { };
            if (args.Length == 2)
                requestHeaders = (IDictionary<string, string>)args[1];

            return requestHeaders;
        }

        public static object ParseContent(string httpMethod, object[] args)
        {
            if (IsImmutableVerb(httpMethod))
                return null;

            if (!IsMutableVerb(httpMethod))
                throw new ArgumentException("HttpMethod not supported");

            if (args.Length == 0)
                return null;

            if (args[0] == null)
                return null;

            if (args[0].GetType().IsPrimitive)
                return null;

            return args[0];
        }

        public static bool TryParseJson(this string json, out object result, Type type = null)
        {
            type = type ?? typeof(object);
            try
            {
                result = JsonConvert.DeserializeObject(json, type);
                return true;
            }
            catch
            {
                result = null;
                return false; //Eat invalid json  
            }
        }

        public static object WrapJToken(this JToken jToken)
        {
            object result = null;

            //JObject
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                result = new RestClientResponseObject(jObject);
            }

            //JValue
            var jValue = jToken as JValue;
            if (jValue != null)
            {
                result = jValue.Value;
            }

            //JArray
            var jArray = jToken as JArray;
            if (jArray != null)
            {
                result = jArray.Select(WrapJToken).ToArray();
            }

            return result;
        }
    }
}
