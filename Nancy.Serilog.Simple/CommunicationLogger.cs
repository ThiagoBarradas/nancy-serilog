using Nancy.Serilog.Simple.Extensions;
using Nancy.Serilog.Simple.Extractors;
using Serilog;
using Serilog.Context;
using System;

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
        /// Log context 
        /// </summary>
        /// <param name="context"></param>
        public void LogData(NancyContext context)
        {
            if (context?.Items == null || context.Items.TryGetValue(DisableLoggingExtension.ITEM_NAME, out object disableSerilog) == false)
            {
                this.LogData(context, null);
            }
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

            var statusCode = context.GetStatusCode(exception);

            string exceptionMessage = null;
            string exceptionStackTrace = null;

            if (exception != null)
            {
                exceptionMessage = HandleFieldSize(exception.Message, 256);
                exceptionStackTrace = HandleFieldSize(exception.StackTrace, 768);
            }

            object controller = "Unknow";
            object action = "Unknow";
            if (context.Items != null) 
            {
                context.Items.TryGetValue("Controller", out controller);
                context.Items.TryGetValue("Action", out action);
            }

            LogContext.PushProperty("RequestBody", context.GetRequestBody(this.NancySerilogConfiguration.Blacklist));
            LogContext.PushProperty("Method", context.Request.Method);
            LogContext.PushProperty("Path", context.Request.Path);
            LogContext.PushProperty("Host", context.Request.Url.HostName);
            LogContext.PushProperty("Port", context.Request.Url.Port);
            LogContext.PushProperty("Url", context.Request.Url);
            LogContext.PushProperty("QueryString", context.Request.Url.Query);
            LogContext.PushProperty("Query", context.GetQueryString());
            LogContext.PushProperty("RequestHeaders", context.GetRequestHeaders());
            LogContext.PushProperty("Ip", context.GetIp());
            LogContext.PushProperty("IsSuccessful", statusCode < 400);
            LogContext.PushProperty("StatusCode", statusCode);
            LogContext.PushProperty("StatusDescription", ((HttpStatusCode)statusCode).ToString());
            LogContext.PushProperty("StatusCodeFamily", context.GetStatusCodeFamily(exception));
            LogContext.PushProperty("ProtocolVersion", context.Request.ProtocolVersion);
            LogContext.PushProperty("ErrorException", exceptionStackTrace);
            LogContext.PushProperty("ErrorMessage", exceptionMessage);
            LogContext.PushProperty("ResponseContent", context.GetResponseContent());
            LogContext.PushProperty("ContentType", context.Response.ContentType);
            LogContext.PushProperty("ContentLength", context.GetResponseLength());
            LogContext.PushProperty("ResponseHeaders", context.GetResponseHeaders());
            LogContext.PushProperty("ElapsedMilliseconds", context.GetExecutionTime());
            LogContext.PushProperty("Version", this.NancySerilogConfiguration.Version);
            LogContext.PushProperty("RequestKey", context.GetRequestKey());
            LogContext.PushProperty("Controller", controller?.ToString());
            LogContext.PushProperty("Operation", action?.ToString());
            LogContext.PushProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

            if (context.Items.ContainsKey("NancySerilogAdditionalInfo"))
            {
                var additionalInfo = (AdditionalInfo) context.Items["NancySerilogAdditionalInfo"];

                if (additionalInfo?.Data != null)
                {
                    foreach (var item in additionalInfo.Data)
                    {
                        LogContext.PushProperty(item.Key, item.Value);
                    }
                }
            }

            

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
        /// Handle field size
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxSize"></param>
        /// <param name="required"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string HandleFieldSize(string value, int maxSize, bool required = false, string defaultValue = "????")
        {
            if (string.IsNullOrWhiteSpace(value) && !required)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultValue;
            }

            if (value.Length > maxSize)
            {
                return value.Substring(0, maxSize);
            }

            return value;
        }
    }
}
