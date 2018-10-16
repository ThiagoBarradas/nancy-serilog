using Nancy.Bootstrapper;
using Nancy.Serilog.Simple.Extensions;
using Nancy.Serilog.Simple.Tests.Mock;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using PackUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WebApi.Models.Exceptions;
using WebApi.Models.Helpers;
using WebApi.Models.Response;
using Xunit;

namespace Nancy.Serilog.Simple.Tests.Extensions
{
    /// <summary>
    /// Pipeline extension test
    /// </summary>
    public static class PipelinesExtensionTest
    {
        [Fact]
        public static void WriteStopwatchAndRequestKey_Should_Not_Have_Effect_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;
            TinyIoCContainer container = new TinyIoCContainer();

            // act
            PipelinesExtension.WriteStopwatchAndRequestKey(context, container);

            // assert
            Assert.Null(context);
        }
        
        [Fact]
        public static void WriteStopwatchAndRequestKey_Should_Create_New_Request_Key()
        {
            // arrange
            NancyContext context = NancyContextMock.Create();
            TinyIoCContainer container = new TinyIoCContainer();

            // act
            PipelinesExtension.WriteStopwatchAndRequestKey(context, container);

            // assert
            Assert.NotNull(context);
            Assert.NotNull(context.Items);
            Assert.Equal(2, context.Items.Count);
            Assert.NotNull(context.Items["RequestKey"]);
            Assert.NotNull(context.Items["Stopwatch"]);
            Assert.Equal(container.Resolve<RequestKey>().Value, context.Items["RequestKey"]);
        }

        [Fact]
        public static void WriteStopwatchAndRequestKey_Should_Use_Request_Key_From_Header()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                    { { "RequestKey", new string[] { "MyRequestKey" } } });
            TinyIoCContainer container = new TinyIoCContainer();

            // act
            PipelinesExtension.WriteStopwatchAndRequestKey(context, container);

            // assert
            Assert.NotNull(context);
            Assert.NotNull(context.Items);
            Assert.Equal(2, context.Items.Count);
            Assert.NotNull(context.Items["RequestKey"]);
            Assert.Equal("MyRequestKey", context.Items["RequestKey"]);
            Assert.NotNull(context.Items["Stopwatch"]);
        }

        [Fact]
        public static void ReadStopwatchAndRequestKey_Should_Not_Have_Effect_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            PipelinesExtension.ReadStopwatchAndRequestKey(context);

            // assert
            Assert.Null(context);
        }

        [Fact]
        public static void ReadStopwatchAndRequestKey_Should_Not_Returns_Headers_When_Response_Is_Null()
        {
            // arrange
            NancyContext context = NancyContextMock.Create();
            context.Response = null;

            // act
            PipelinesExtension.ReadStopwatchAndRequestKey(context);

            // assert
            Assert.NotNull(context);
            Assert.NotNull(context.Response);
            Assert.False(context.Response.Headers.ContainsKey("RequestKey"));
            Assert.False(context.Response.Headers.ContainsKey("X-Internal-Time"));
        }

        [Fact]
        public static void WriteStopwatchAndRequestKey_Should_Not_Returns_Headers_When_Items_Not_Exists()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseContent: "some response",
                responseHeaders: new Dictionary<string, string>());
            TinyIoCContainer container = new TinyIoCContainer();

            // act
            PipelinesExtension.WriteStopwatchAndRequestKey(context, container);

            // assert
            Assert.NotNull(context);
            Assert.NotNull(context.Response);
            Assert.False(context.Response.Headers.ContainsKey("RequestKey"));
            Assert.False(context.Response.Headers.ContainsKey("X-Internal-Time"));
        }

        [Fact]
        public static void ReadStopwatchAndRequestKey_Should_Returns_Headers_When_Items_Exists()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseContent: "some response",
                responseHeaders: new Dictionary<string, string>());

            context.Items.Add("Stopwatch", Stopwatch.StartNew());
            context.Items.Add("RequestKey", "1234567890");

            // act
            PipelinesExtension.ReadStopwatchAndRequestKey(context);

            // assert
            Assert.NotNull(context);
            Assert.NotNull(context.Response);
            Assert.True(context.Response.Headers.ContainsKey("RequestKey"));
            Assert.True(context.Response.Headers.ContainsKey("X-Internal-Time"));
            Assert.True(int.Parse(context.Response.Headers["X-Internal-Time"]) >= 0);
            Assert.Equal("1234567890",context.Response.Headers["RequestKey"]);
        }

        [Fact]
        public static void AddStopwatchAndRequestKey_Should_Add_Pipelines()
        {
            // arrange
            var pipelines = new Pipelines();
            var context = NancyContextMock.Create(
                responseContent: "{ \"test\": 1 }",
                responseHeaders: new Dictionary<string, string>
                    { { "Content-Type", "application/json" } });
            TinyIoCContainer container = new TinyIoCContainer();

            // act
            pipelines.AddStopwatchAndRequestKeyPipelines(container);
            pipelines.BeforeRequest.Invoke(context, new CancellationToken());
            pipelines.AfterRequest.Invoke(context, new CancellationToken());
            context.Response.Headers.Remove("X-Internal-Time");
            context.Response.Headers.Remove("RequestKey");
            pipelines.OnError.Invoke(context, new Exception());

            // assert
            Assert.Single(pipelines.BeforeRequest.PipelineItems);
            Assert.Single(pipelines.AfterRequest.PipelineItems);
            Assert.Single(pipelines.OnError.PipelineItems);
        }

        [Fact]
        public static void AddHandleSuccessfulRequestsPipelines_Should_Add_Pipelines()
        {
            // arrange
            var pipelines = new Pipelines();
            var container = new TinyIoCContainer();
            container.Register<ICommunicationLogger, CommunicationLoggerMock>();
            var context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                { { "Content-Type", "text/plain" } });

            // act
            pipelines.AddHandlerSuccessfulRequestsPipelines(container);
            pipelines.AfterRequest.Invoke(context, new CancellationToken());

            // assert
            Assert.Single(pipelines.AfterRequest.PipelineItems);
            Assert.Empty(pipelines.BeforeRequest.PipelineItems);
            Assert.Empty(pipelines.OnError.PipelineItems);
        }

        [Fact]
        public static void AddHandlerExceptionsPipelines_Should_Add_Pipelines()
        {
            // arrange
            var pipelines = new Pipelines();
            var container = new TinyIoCContainer();
            container.Register<ICommunicationLogger, CommunicationLoggerMock>();
            container.Register(JsonUtility.CamelCaseJsonSerializerSettings);
            var context = NancyContextMock.Create();

            // act
            pipelines.AddHandlerExceptionsPipelines(container);
            pipelines.OnError.Invoke(context, null);

            // assert
            Assert.Single(pipelines.OnError.PipelineItems);
            Assert.Empty(pipelines.AfterRequest.PipelineItems);
            Assert.Empty(pipelines.BeforeRequest.PipelineItems);
        }

        [Fact]
        public static void HandleExceptions_Log_Exception()
        {
            // arrange
            var logger = new CommunicationLoggerMock();
            var jsonSerializer = JsonUtility.CamelCaseJsonSerializerSettings;
            var exception = new Exception();
            var context = NancyContextMock.Create();
            
            // act
            var response = PipelinesExtension.HandleExceptions(context, exception, jsonSerializer, logger);
            
            // assert
            Assert.Null(response);
            Assert.Single(logger.Logs);
            Assert.True(bool.Parse(logger.Logs.First().Items["WorksWithException"].ToString()));
        }

        [Fact]
        public static void HandleExceptions_Log_Not_Have_Effect_And_Returns_Null_When_Context_Is_Null()
        {
            // arrange
            var logger = new CommunicationLoggerMock();
            var jsonSerializer = JsonUtility.CamelCaseJsonSerializerSettings;
            var exception = new BadRequestException();
            NancyContext context = null;

            // act
            var response = PipelinesExtension.HandleExceptions(context, exception, jsonSerializer, logger);

            // assert
            Assert.Null(response);
            Assert.Empty(logger.Logs);
        }

        [Fact]
        public static void HandleExceptions_Log_ApiException_When_Content_Is_Null()
        {
            // arrange
            var logger = new CommunicationLoggerMock();
            var jsonSerializer = JsonUtility.CamelCaseJsonSerializerSettings;
            var exception = new BadRequestException();
            var context = NancyContextMock.Create();

            // act
            var response = PipelinesExtension.HandleExceptions(context, exception, jsonSerializer, logger);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Single(logger.Logs);
            Assert.True(bool.Parse(logger.Logs.First().Items["WorksWithoutException"].ToString()));
        }

        [Fact]
        public static void HandleExceptions_Log_ApiException_When_Content_Is_Not_Null_And_Context_Response_Is_Null()
        {
            // arrange
            var logger = new CommunicationLoggerMock();
            var jsonSerializer = JsonUtility.CamelCaseJsonSerializerSettings;
            var errors = ErrorsResponse.WithSingleError("someerror", "someproperty");
            var exception = new BadRequestException(errors);
            NancyContext context = NancyContextMock.Create();
            context.Response = null;

            // act
            var response = PipelinesExtension.HandleExceptions(context, exception, jsonSerializer, logger);

            // assert
            Assert.NotNull(response);
            Assert.Equal("application/json", response.ContentType);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Single(logger.Logs);
            Assert.True(bool.Parse(logger.Logs.First().Items["WorksWithoutException"].ToString()));
        }

        [Fact]
        public static void AddLogPipelines_Should_Throws_Exception_When_Pipeline_Is_Null()
        {
            // arrange
            IPipelines pipelines = null;
            TinyIoCContainer container = new TinyIoCContainer();

            // act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                pipelines.AddLogPipelines(container));

            // assert
            Assert.Equal("Value cannot be null.\r\nParameter name: pipelines", exception.Message);
        }

        [Fact]
        public static void AddLogPipelines_Should_Throws_Exception_When_Container_Is_Null()
        {
            // arrange
            IPipelines pipelines = new Pipelines();
            TinyIoCContainer container = null;

            // act
            var exception = Assert.Throws<ArgumentNullException>(() => 
                pipelines.AddLogPipelines(container));

            // assert
            Assert.Equal("Value cannot be null.\r\nParameter name: container", exception.Message);
        }

        [Fact]
        public static void AddLogPipelines_Should_Add_Pipelines()
        {
            // arrange
            IPipelines pipelines = new Pipelines();
            TinyIoCContainer container = new TinyIoCContainer();
            container.Register<ICommunicationLogger, CommunicationLoggerMock>();
            container.Register(JsonUtility.CamelCaseJsonSerializerSettings);

            // act
            pipelines.AddLogPipelines(container);

            // assert
            Assert.Single(pipelines.BeforeRequest.PipelineItems);
            Assert.Equal(2, pipelines.AfterRequest.PipelineItems.ToList().Count);
            Assert.Equal(2, pipelines.OnError.PipelineItems.ToList().Count);
        }

    } 
}
