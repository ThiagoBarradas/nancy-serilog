using Moq;
using Nancy.Bootstrapper;
using Nancy.Serilog.Simple.Extensions;
using Nancy.Serilog.Simple.Tests.Mock;
using Nancy.TinyIoc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using LoggerDebug = Serilog.Debugging;

namespace Nancy.Serilog.Simple.Tests
{
    public class CommunicationLoggerTest
    {
        private TestOutputHelper TestOutputHelper { get; set; }

        public CommunicationLoggerTest(ITestOutputHelper testOutputHelper)
        {
            this.TestOutputHelper = testOutputHelper as TestOutputHelper;

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Domain", "Nancy.Serilog.Simple")
                .Enrich.WithProperty("Application", "CommunicationLogger")
                .MinimumLevel.Verbose()
                .WriteTo.XunitTestOutput(this.TestOutputHelper)
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            LoggerDebug.SelfLog.Enable(msg => Debug.WriteLine(msg));
        }

        [Fact]
        public void CommunicationLogger_Should_Create_Object_With_Configuration()
        {
            // arrange
            var config = new NancySerilogConfiguration
            {
                Blacklist = new string[] { "test" },
                ErrorTitle = "Error title",
                InformationTitle = "Information title",
                Logger = Log.Logger
            };

            // act
            var logger = new CommunicationLogger(config);

            // assert
            Assert.NotNull(logger);
            Assert.NotNull(logger.NancySerilogConfiguration);
            Assert.NotNull(logger.NancySerilogConfiguration.Blacklist);
            Assert.NotNull(logger.NancySerilogConfiguration.ErrorTitle);
            Assert.NotNull(logger.NancySerilogConfiguration.InformationTitle);
            Assert.NotNull(logger.NancySerilogConfiguration.Logger);
            Assert.Single(logger.NancySerilogConfiguration.Blacklist);
            Assert.Equal("test", logger.NancySerilogConfiguration.Blacklist.FirstOrDefault());
            Assert.Equal("Error title", logger.NancySerilogConfiguration.ErrorTitle);
            Assert.Equal("Information title", logger.NancySerilogConfiguration.InformationTitle);

        }

        [Fact]
        public void CommunicationLogger_Should_Create_Object_With_Null_Configuration()
        {
            // arrange
            NancySerilogConfiguration config = null;

            // act
            var logger = new CommunicationLogger(config);

            // assert
            Assert.NotNull(logger);
            Assert.NotNull(logger.NancySerilogConfiguration);
            Assert.Null(logger.NancySerilogConfiguration.Blacklist);
            Assert.Null(logger.NancySerilogConfiguration.ErrorTitle);
            Assert.Null(logger.NancySerilogConfiguration.InformationTitle);
            Assert.NotNull(logger.NancySerilogConfiguration.Logger);
        }

        [Fact]
        public void CommunicationLogger_Should_Create_Object_With_Empty_Constructor()
        {
            // arranges & act
            var logger = new CommunicationLogger();

            // assert
            Assert.NotNull(logger);
            Assert.NotNull(logger.NancySerilogConfiguration);
            Assert.Null(logger.NancySerilogConfiguration.Blacklist);
            Assert.Null(logger.NancySerilogConfiguration.ErrorTitle);
            Assert.Null(logger.NancySerilogConfiguration.InformationTitle);
            Assert.NotNull(logger.NancySerilogConfiguration.Logger);
        }
        
        [Fact]
        public void LogData_Should_Throws_Exception_When_Context_Is_Null()
        {
            // arrange
            var logger = new CommunicationLogger();

            // act
            Exception ex = Assert.Throws<ArgumentNullException>(() =>
                logger.LogData(null));

            // assert
            Assert.Equal("Value cannot be null.\r\nParameter name: context", ex.Message);
        }

        [Fact]
        public void LogData_Should_Work_With_X_Forwarded_For_And_X_Internal_Time()
        {
            // arrange
            var originIp = "127.0.0.1";
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "{ \"test\" : \"123\" }";
            var requestUrl = "http://localhost/test";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                {  "X-Forwarded-For", new string[] { "226.225.223.224" } },
                {  "Content-Type", new string[] { "application/json" } },
                {  "Accept", new string[] { "application/json", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "application/json" },
                {  "X-Internal-Time", "100" }
            };
            var responseStatusCode = HttpStatusCode.Created;
            var Content = "{ \"xpto\" : \"test\" }";

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                InformationTitle = "{Method} | {Ip} | {StatusCode} | {StatusCodeFamily} | {ErrorException} | {ElapsedMilliseconds}"
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context);

            // assert
            Assert.Contains("[Information] \"POST\" | \"226.225.223.224\" | 201 | \"2XX\" | null | 100", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_X_Www_Url_Form_Encoded()
        {
            // arrange
            var originIp = "127.0.0.1";
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "someproperty=somevalue&somep1=somevalue1&somep1=somevalue2";
            var requestUrl = "http://localhost/test";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                {  "X-Forwarded-For", new string[] { "226.225.223.224" } },
                {  "Content-Type", new string[] { "application/x-www-form-urlencoded" } },
                {  "Accept", new string[] { "application/json", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "application/json" },
                {  "X-Internal-Time", "100" }
            };
            var responseStatusCode = HttpStatusCode.Created;
            var Content = "{ \"xpto\" : \"test\" }";

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                InformationTitle = "XWWW {Method} | {Ip} | {StatusCode} | {StatusCodeFamily} | {ErrorException} | {ElapsedMilliseconds}"
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context);

            // assert
            Assert.Contains("[Information] XWWW \"POST\" | \"226.225.223.224\" | 201 | \"2XX\" | null | 100", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_Blacklist_And_Information()
        {
            // arrange
            var originIp = "127.0.0.1";
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "{ \"test\" : \"123\", \"test2\" : \"123\" }";
            var requestUrl = "http://localhost/test?query1=xpto1&query1=xpto2&query2=1&query3=";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                {  "Content-Type", new string[] { "application/json" } },
                {  "Accept", new string[] { "application/json", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "application/json" },
            };
            var responseStatusCode = HttpStatusCode.BadRequest;
            var Content = "{ \"xpto\" : \"test\" }";

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);
            context.Items["test"] = "test";

            var config = new NancySerilogConfiguration
            {
                InformationTitle = "{Method} | {Host} | {Path} | {Ip} | {IsSuccessful} | {ProtocolVersion} | {StatusCode} | {StatusDescription} | {StatusCodeFamily} | {ElapsedMilliseconds}",
                Blacklist = new string[] { "test" },
                Logger = Log.Logger
            };

            var logger = new CommunicationLogger(config);
            IPipelines pipelines = new Pipelines();
            TinyIoCContainer container = new TinyIoCContainer();

            container.Register<ICommunicationLogger>(logger);
            container.Register(PackUtils.JsonUtility.CamelCaseJsonSerializerSettings);

            pipelines.AddLogPipelines(container);

            // act
            logger.LogData(context);

            // assert
            Assert.Contains("[Information] \"POST\" | \"localhost\" | \"/test\" | \"127.0.0.1\" | False | \"1.1\" | 400 | \"BadRequest\" | \"4XX\" | \"??\"", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_Blacklist_And_Information_With_Default_Title()
        {
            // arrange
            var originIp = "127.0.0.1";
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "{ \"test\" : \"123\", \"test2\" : \"123\" }";
            var requestUrl = "http://localhost/test?query1=xpto1&query1=xpto2&query2=1&query3";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                { "RequestKey", new string[] { Guid.NewGuid().ToString() }  },
                {  "Content-Type", new string[] { "application/json" } },
                {  "Accept", new string[] { "application/json", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "application/json" },
            };
            var responseStatusCode = HttpStatusCode.BadRequest;
            var Content = "{ \"xpto\" : \"test\" }";

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                Blacklist = new string[] { "test" }
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context);

            // assert
            Assert.Contains("[Information] HTTP \"POST\" \"/test\" from \"127.0.0.1\" responded 400 in \"??\" ms", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_Exception_Without_Ip_And_Response_Body()
        {
            // arrange
            string originIp = null;
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "{ \"test\" : \"123\" }";
            var requestUrl = "http://some.thing.xp/test?sometest=rrtt";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                {  "Content-Type", new string[] { "text/html" } },
                {  "Accept", new string[] { "text/html", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "text/html" },
            };
            var responseStatusCode = HttpStatusCode.BadRequest;
            string Content = null;

            var exception = new ArgumentNullException("test");

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                ErrorTitle = "{Method} | {Ip} | {StatusCode} | {StatusCodeFamily} | {ElapsedMilliseconds}",
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context, exception);

            // assert
            Assert.Contains("[Error] \"POST\" | \"??\" | 500 | \"5XX\" | \"??\"", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_Exception_With_Default_Title()
        {
            // arrange
            string originIp = null;
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            string requestBody = null;
            var requestUrl = "http://some.thing.xp/test?sometest=rrtt";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                {  "Content-Type", new string[] { "text/html" } },
                {  "Accept", new string[] { "text/html", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "text/html" },
            };
            var responseStatusCode = HttpStatusCode.BadRequest;
            string Content = null;

            var exception = new ArgumentNullException("test");

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var logger = new CommunicationLogger();

            // act
            logger.LogData(context, exception);

            // assert
            Assert.Contains("[Error] HTTP \"POST\" \"/test\" from \"??\" responded 500 in \"??\" ms", this.TestOutputHelper.Output);
        }

        [Fact]
        public void DisableLog_Should_Works()
        {
            // arrange
            string originIp = null;
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            string requestBody = null;
            var requestUrl = "http://some.thing.xp/log-disabled?sometest=rrtt";
            var requestHeaders = new Dictionary<string, IEnumerable<string>>
            {
                {  "Content-Type", new string[] { "text/html" } },
                {  "Accept", new string[] { "text/html", "text/xml" } }
            };

            var responseHeaders = new Dictionary<string, string>
            {
                {  "Content-Type", "text/html" },
            };
            var responseStatusCode = HttpStatusCode.BadRequest;
            string Content = null;

            var context = NancyContextMock.Create(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var logger = new CommunicationLogger();

            var module = new MyModule();
            module.Context = context;
            module.DisableLogging();

            // act
            logger.LogData(context);

            // assert
            Assert.DoesNotContain("log-disabled", this.TestOutputHelper.Output);
        }


    }

    public class MyModule : NancyModule
    {

    }
}
