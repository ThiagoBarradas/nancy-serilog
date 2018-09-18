using JsonMasking;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Context;
using System;
using System.IO;
using System.Linq;

namespace Nancy.Serilog.Simple
{
    /// <summary>
    /// Communication Logger implementation
    /// </summary>
    public class CommunicationLogger : ICommunicationLogger
    {
        /// <summary>
        /// Default Log Information Title
        /// </summary>
        public const string DefaultInformationTitle = "HTTP {Method} {Path} from {Ip} responded {StatusCode} in {ElapsedMilliseconds} ms";

        /// <summary>
        /// Default Log Error Title
        /// </summary>
        public const string DefaultErrorTitle = "HTTP {Method} {Path} from {Ip} responded {StatusCode} in {ElapsedMilliseconds} ms";

        /// <summary>
        /// Nancy Serilog Configuration
        /// </summary>
        public NancySerilogConfiguration NancySerilogConfiguration { get; set; }

        /// <summary>
        /// Constructor with configuration
        /// </summary>
        /// <param name="logger"></param>
        public CommunicationLogger(NancySerilogConfiguration configuration)
        {
            this.SetupCommunicationLogger(configuration);
        }

        /// <summary>
        /// Constructor using global logger definition
        /// </summary>
        public CommunicationLogger()
        {
            this.SetupCommunicationLogger(null);
        }

        /// <summary>
        /// Configure nancy pipeline
        /// </summary>
        /// <param name="pipelines"></param>
        public void ConfigurePipelines(IPipelines pipelines)
        {
            if (pipelines == null)
            {
                throw new ArgumentNullException(nameof(pipelines));
            }

            pipelines.OnError.AddItemToEndOfPipeline((context, exception) =>
            {
                this.LogData(context, exception);
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline((context) =>
            {
                this.LogData(context);
            });
        }

        /// <summary>
        /// Log context 
        /// </summary>
        /// <param name="context"></param>
        public void LogData(NancyContext context)
        {
            this.LogData(context, null);
        }

        /// <summary>
        /// Log context and exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public void LogData(NancyContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var statusCode = this.GetStatusCode(context, exception);

            LogContext.PushProperty("Body", this.GetRequestBodyAsString(context));
            LogContext.PushProperty("Method", context.Request.Method);
            LogContext.PushProperty("Path", context.Request.Path);
            LogContext.PushProperty("Host", context.Request.Url.HostName);
            LogContext.PushProperty("UrlBase", context.Request.Url.SiteBase);
            LogContext.PushProperty("Query", context.Request.Url.Query);
            LogContext.PushProperty("RequestHeaders", this.GetRequestHeadersAsJson(context));
            LogContext.PushProperty("Ip", this.GetIp(context));
            LogContext.PushProperty("IsSuccessful", statusCode < 400);
            LogContext.PushProperty("StatusCode", statusCode);
            LogContext.PushProperty("StatusDescription", ((HttpStatusCode) statusCode).ToString());
            LogContext.PushProperty("StatusCodeFamily", this.GetStatusCodeFamily(statusCode));
            LogContext.PushProperty("ProtocolVersion", context.Request.ProtocolVersion);
            LogContext.PushProperty("ErrorException", exception);
            LogContext.PushProperty("ErrorMessage", exception?.Message);
            LogContext.PushProperty("Content", this.GetResponseAsString(context));
            LogContext.PushProperty("ContentType", context.Response.ContentType);
            LogContext.PushProperty("ContentLength", this.GetResponseLength(context));
            LogContext.PushProperty("ResponseHeaders", this.GetResponsetHeadersAsJson(context));
            LogContext.PushProperty("ElapsedMilliseconds", this.GetExecutionTime(context, exception));

            if (exception != null || statusCode >= 500)
            {
                var errorTitle = this.NancySerilogConfiguration.ErrorTitle ?? DefaultErrorTitle;
                this.NancySerilogConfiguration.Logger.Error(errorTitle);
            }
            else
            {
                var informationTitle = this.NancySerilogConfiguration.InformationTitle ?? DefaultInformationTitle;
                this.NancySerilogConfiguration.Logger.Information(informationTitle);
            }
        }
        
        /// <summary>
        /// Initialize instance
        /// </summary>
        /// <param name="configuration"></param>
        private void SetupCommunicationLogger(NancySerilogConfiguration configuration)
        {
            this.NancySerilogConfiguration =
                configuration ?? new NancySerilogConfiguration();

            this.NancySerilogConfiguration.Logger =
                configuration?.Logger ?? Log.Logger;
        }

        /// <summary>
        /// Get status code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private int GetStatusCode(NancyContext context, Exception exception)
        {
            if (exception != null)
            {
                return 500;
            }

            var statusCode = (int)context.Response.StatusCode;
            return statusCode;
        }

        /// <summary>
        /// Get request body
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetRequestBodyAsString(NancyContext context)
        {
            var body = RequestStream.FromStream(context.Request.Body).AsString();
            var contentType = context.Request.Headers["Content-Type"].FirstOrDefault() ?? "";
            var isJson = contentType.Contains("application/json");

            if (isJson && this.NancySerilogConfiguration.Blacklist?.Any() == true)
            {
                return body.MaskFields(this.NancySerilogConfiguration.Blacklist, "******");
            }

            return body;
        }

        /// <summary>
        /// Get ip (X-Forwarded-For or original)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetIp(NancyContext context)
        {
            string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                        ?? context.Request.UserHostAddress;

            return ip ?? "??";
        }
        
        /// <summary>
        /// Get status code family, like 1XX 2XX 3XX 4XX 5XX
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private string GetStatusCodeFamily(int statusCode)
        {
            var family = statusCode.ToString()[0] + "XX";

            return family;
        }

        /// <summary>
        /// Get total execution time from X-Internal-Time header
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private object GetExecutionTime(NancyContext context, Exception exception)
        {
            string elapsedDefault = "??";

            if (exception != null)
            {
                return elapsedDefault;
            }
            
            if (context.Response.Headers.TryGetValue("X-Internal-Time", out string elapsedParsed) == true)
            {
                if (Int64.TryParse(elapsedParsed, out long elapsedLong) == true)
                {
                    return elapsedLong;
                }
            }
            
            return elapsedDefault;
        }

        /// <summary>
        /// Get all request headers as json
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetRequestHeadersAsJson(NancyContext context)
        {
            var headers = new JObject();
            foreach (var item in context.Request.Headers)
            {
                var value = item.Value.FirstOrDefault();

                if (item.Value.Count() > 1)
                {
                    value = string.Join(",", item.Value);
                }
                
                headers.Add(new JProperty(item.Key, value));
            }
            return JsonConvert.SerializeObject(headers, Formatting.Indented);
        }

        /// <summary>
        /// Get all response headers as json
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetResponsetHeadersAsJson(NancyContext context)
        {
            var headers = new JObject();
            foreach (var item in context.Response.Headers)
            {
                JProperty jProperty = new JProperty(item.Key, item.Value);
                headers.Add(jProperty);
            }
            return JsonConvert.SerializeObject(headers, Formatting.Indented);
        }

        /// <summary>
        /// Get response as string 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetResponseAsString(NancyContext context)
        {
            var stream = new MemoryStream();
            context.Response.Contents.Invoke(stream);

            stream.Position = 0;
            string responseContent = string.Empty;
            using (var reader = new StreamReader(stream))
            {
                responseContent = reader.ReadToEnd();
            }

            return responseContent;
        }

        /// <summary>
        /// Get content length
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private long GetResponseLength(NancyContext context)
        {
            var stream = new MemoryStream();
            context.Response.Contents.Invoke(stream);
            stream.Position = 0;
            return stream.Length;
        }
    }
}
