using JsonMasking;
using Nancy.Extensions;
using Nancy.IO;
using PackUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nancy.Serilog.Simple.Extractors
{
    /// <summary>
    /// Nancy context extractor
    /// </summary>
    public static class NancyContextExtractor
    {
        /// <summary>
        /// Get status code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static int GetStatusCode(this NancyContext context, Exception exception)
        {
            if (exception != null)
            {
                return 500;
            }

            var statusCode = context?.Response?.StatusCode ?? 0;
            return (int) statusCode;
        }

        /// <summary>
        /// Get status code family, like 1XX 2XX 3XX 4XX 5XX
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetStatusCodeFamily(this NancyContext context, Exception exception)
        {
            var statusCode = context.GetStatusCode(exception);
            return statusCode.ToString()[0] + "XX";
        }

        /// <summary>
        /// Get query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetQueryString(this NancyContext context)
        {
            return context?.Request?.Query.ToDictionary();
        }

        /// <summary>
        /// Get all request headers
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetRequestHeaders(this NancyContext context)
        {
            if (context?.Request?.Headers == null)
            {
                return null;
            }

            var headers = new Dictionary<string, object>();

            foreach (var item in context.Request.Headers)
            {
                var value = (item.Value != null) 
                    ? string.Join(",", item.Value) 
                    : string.Empty;
                headers.Add(item.Key, value);
            }

            return headers;
        }

        /// <summary>
        /// Get all response headers
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetResponseHeaders(this NancyContext context)
        {
            if (context?.Response?.Headers == null)
            {
                return null;
            }

            var headers = new Dictionary<string, object>();

            foreach (var item in context.Response.Headers)
            {
                headers.Add(item.Key, item.Value);
            }

            return headers;
        }

        /// <summary>
        /// Get total execution time from X-Internal-Time header
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static object GetExecutionTime(this NancyContext context)
        {
            long elapsedDefault = -1;
            string elapsedParsed = "-1";

            context?.Response?.Headers?.TryGetValue("X-Internal-Time", out elapsedParsed);
            if (Int64.TryParse(elapsedParsed, out long elapsedLong) == true)
            {
                return elapsedLong;
            }

            return elapsedDefault;
        }

        /// <summary>
        /// Get request key from RequestKey Header
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRequestKey(this NancyContext context)
        {
            if (context?.Response?.Headers?.ContainsKey("RequestKey") == true)
            {
                return context.Response.Headers["RequestKey"];
            }

            return null;
        }

        /// <summary>
        /// Get ip (X-Forwarded-For or original)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetIp(this NancyContext context)
        {
            var deafultIp = "??";

            if (context?.Request == null)
            {
                return deafultIp;
            }

            if (context.Request.Headers.Any(r => r.Key == "X-Forwarded-For") == true)
            {
                return context.Request.Headers["X-Forwarded-For"].First();
            }

            return context.Request.UserHostAddress ?? deafultIp;
        }

        /// <summary>
        /// Get request body
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static object GetRequestBody(this NancyContext context, string[] blacklist)
        {
            if (context?.Request == null)
            {
                return null;
            }

            string body = RequestStream.FromStream(context.Request.Body).AsString();

            var contentType = (context.Request.Headers.Keys.Contains("Content-Type") == true)
                ? string.Join(";", context.Request.Headers["Content-Type"])
                : string.Empty;

            var isJson = (contentType.Contains("json") == true);
            var isForm = (context.Request.Form.Count > 0);
            if (isJson)
            {
                return GetContentAsObjectByContentTypeJson(body, true, blacklist);
            }
            else if (isForm)
            {
                return context.Request.Form.ToDictionary();
            }
            else
            {
                return new { raw_body = body };
            }
        }
        
        /// <summary>
        /// Get response content
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static object GetResponseContent(this NancyContext context)
        {
            if (context?.Response == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            context.Response.Contents.Invoke(stream);
            stream.Position = 0;

            string responseContent = string.Empty;
            using (var reader = new StreamReader(stream))
            {
                responseContent = reader.ReadToEnd();
            }
            stream.Dispose();

            if (context.Response.ContentType.Contains("json") == true &&
                string.IsNullOrWhiteSpace(responseContent) == false)
            {
                return GetContentAsObjectByContentTypeJson(responseContent, false, null);
            }
            else
            {
                return new { raw_content = responseContent };
            }
        }

        /// <summary>
        /// Get content length
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static long GetResponseLength(this NancyContext context)
        {
            if (context?.Response?.Contents == null)
            {
                return 0;
            }

            var stream = new MemoryStream();
            context.Response.Contents.Invoke(stream);
            stream.Position = 0;
            var length = stream.Length;
            stream.Dispose();
            return length;
        }

        /// <summary>
        /// Get content as object by content type
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        internal static object GetContentAsObjectByContentTypeJson(string content, bool maskJson, string[] backlist)
        {
            try
            {
                if (maskJson == true && backlist?.Any() == true)
                {
                    content = content.MaskFields(backlist, "******");
                }

                return content.DeserializeAsObject();
            }
            catch (Exception) { }

            return content;
        }
    }
}
