using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using PackUtils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using WebApi.Models.Exceptions;
using WebApi.Models.Helpers;

[assembly: InternalsVisibleTo("Nancy.Serilog.Simple.Tests")]
namespace Nancy.Serilog.Simple.Extensions
{
    /// <summary>
    /// Pipelines Extesion
    /// </summary>
    public static class PipelinesExtension
    {
        /// <summary>
        /// Configure nancy pipeline
        /// </summary>
        /// <param name="pipelines"></param>
        public static void AddLogPipelines(this IPipelines pipelines, TinyIoCContainer container)
        {
            if (pipelines == null)
            {
                throw new ArgumentNullException(nameof(pipelines));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            pipelines.AddStopwatchAndRequestKeyPipelines();
            pipelines.AddHandlerExceptionsPipelines(container);
            pipelines.AddHandlerSuccessfulRequestsPipelines(container);           
        }

        /// <summary>
        /// Add stopwatch and add request key
        /// </summary>
        /// <param name="pipelines"></param>
        internal static void AddStopwatchAndRequestKeyPipelines(this IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline((context) => 
                WriteStopwatchAndRequestKey(context));

            pipelines.AfterRequest.AddItemToStartOfPipeline(context =>
                ReadStopwatchAndRequestKey(context));

            pipelines.OnError.AddItemToStartOfPipeline((context, exception) => 
                ReadStopwatchAndRequestKey(context));
        }

        /// <summary>
        /// Write stopwatch and request key headers
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Response WriteStopwatchAndRequestKey(NancyContext context)
        {
            if (context != null)
            {
                context.Items["RequestKey"] = context.Request.Headers.Any(r => r.Key == "RequestKey")
                    ? context.Request.Headers["RequestKey"].First()
                    : Guid.NewGuid().ToString();

                context.Items.Add("Stopwatch", Stopwatch.StartNew());
            }

            return null;
        }

        /// <summary>
        /// Read stopwatch and request key headers
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Response ReadStopwatchAndRequestKey(NancyContext context)
        {
            if (context == null)
            {
                return null;
            }

            var response = context.Response ?? new Response();

            context.Items.TryGetValue("Stopwatch", out object objStopwatch);
            if (objStopwatch != null)
            {
                Stopwatch stopwatch = (Stopwatch)objStopwatch;
                stopwatch.Stop();

                response.Headers.Add("X-Internal-Time", stopwatch.ElapsedMilliseconds.ToString());
            }

            context.Items.TryGetValue("RequestKey", out object objRequestKey);
            if (objRequestKey != null)
            {
                response.Headers.Add("RequestKey", objRequestKey.ToString());
            }

            context.Items.TryGetValue("AccountId", out object objAccountId);
            if (objAccountId != null)
            {
                response.Headers.Add("AccountId", objAccountId.ToString());
            }

            context.Response = response;

            return null;
        }

        /// <summary>
        /// Handler exceptions response
        /// </summary>
        /// <param name="pipelines"></param>
        /// <param name="container"></param>
        internal static void AddHandlerExceptionsPipelines(this IPipelines pipelines, TinyIoCContainer container)
        {
            var jsonSerializer = container.Resolve<JsonSerializerSettings>();
            var logger = container.Resolve<ICommunicationLogger>();

            pipelines.OnError.AddItemToEndOfPipeline((context, exception) =>
            {
                return HandleExceptions(context, exception, jsonSerializer, logger);
            });
        }

        /// <summary>
        /// Hande exceptions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <param name="jsonSerializer"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static Response HandleExceptions(NancyContext context, Exception exception, JsonSerializerSettings jsonSerializer, ICommunicationLogger logger)
        {
            if (context == null)
            {
                return null;
            }

            if (exception is ApiException apiException)
            {
                var apiResponse = apiException.ToApiResponse();

                Response response = (apiResponse.Content != null)
                    ? JsonConvert.SerializeObject(apiResponse.Content, jsonSerializer)
                    : new Response();

                response.ContentType = "application/json";
                response.StatusCode = apiResponse.StatusCode.ConvertToEnum<HttpStatusCode>();
                response.Headers = context.Response?.Headers ?? response.Headers;
                context.Response = response;

                logger.LogData(context);

                return response;
            }

            logger.LogData(context, exception);
            return null;
        }

        /// <summary>
        /// Handle successful requests
        /// </summary>
        /// <param name="pipelines"></param>
        /// <param name="container"></param>
        internal static void AddHandlerSuccessfulRequestsPipelines(this IPipelines pipelines, TinyIoCContainer container)
        {
            var logger = container.Resolve<ICommunicationLogger>();

            pipelines.AfterRequest.AddItemToEndOfPipeline((context) =>
            {
                logger.LogData(context);
            });
        }
    }
}
