using Moq;
using Nancy.Bootstrapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nancy.Serilog.Simple.Tests
{
    public class CommunicationLoggerTest
    {
        private TestOutputHelper TestOutputHelper { get; set; }

        public CommunicationLoggerTest(ITestOutputHelper testOutputHelper)
        {
            this.TestOutputHelper = testOutputHelper as TestOutputHelper;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.XunitTestOutput(this.TestOutputHelper)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Domain", "Nancy.Serilog.Simple")
                .Enrich.WithProperty("Application", "CommunicationLogger")
                .CreateLogger();
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
        public void ConfigurePipelines_Should_Throws_Exception_When_Pipelines_Is_Null()
        {
            // arrange
            var logger = new CommunicationLogger();

            // act
            Exception ex = Assert.Throws<ArgumentNullException>(() =>
                logger.ConfigurePipelines(null));

            // assert
            Assert.Equal("Value cannot be null.\r\nParameter name: pipelines", ex.Message);
        }

        [Fact]
        public void ConfigurePipelines_Should_Define_Error_And_AfterRequest_Pipelines()
        {
            // arrange
            var logger = new CommunicationLogger();

            var afterPipeline = new AfterPipeline();
            var errorPipeline = new ErrorPipeline();
            var beforePipeline = new BeforePipeline();
            var pipelinesMock = new Mock<IPipelines>();

            pipelinesMock
                .Setup(m => m.AfterRequest)
                .Returns(afterPipeline);
            pipelinesMock
                .Setup(m => m.OnError)
                .Returns(errorPipeline);
            pipelinesMock
                .Setup(m => m.BeforeRequest)
                .Returns(beforePipeline);

            IPipelines pipelines = pipelinesMock.Object;


            // act
            logger.ConfigurePipelines(pipelines);

            // assert
            Assert.NotEmpty(afterPipeline.PipelineItems);
            Assert.NotEmpty(errorPipeline.PipelineItems);
            Assert.Empty(beforePipeline.PipelineItems);
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

            var context = this.GetNancyContext(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                InformationTitle = "{Method} | {Body} | [{Path}] | {RequestHeaders} | {Ip} | {StatusCode} | {StatusCodeFamily} | {ErrorException} | {Content} | {ResponseHeaders} | {ElapsedMilliseconds}"
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context);

            // assert
            Assert.Contains("[Information] \"POST\" | \"{ \\\"test\\\" : \\\"123\\\" }\" | [\"/test\"] | \"{\r\n  \\\"X-Forwarded-For\\\": \\\"226.225.223.224\\\",\r\n  \\\"Content-Type\\\": \\\"application/json\\\",\r\n  \\\"Accept\\\": \\\"application/json,text/xml\\\"\r\n}\" | \"226.225.223.224\" | 201 | \"2XX\" | null | \"{ \\\"xpto\\\" : \\\"test\\\" }\" | \"{\r\n  \\\"Content-Type\\\": \\\"application/json\\\",\r\n  \\\"X-Internal-Time\\\": \\\"100\\\"\r\n}\" | 100", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_Blacklist_And_Information()
        {
            // arrange
            var originIp = "127.0.0.1";
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "{ \"test\" : \"123\", \"test2\" : \"123\" }";
            var requestUrl = "http://localhost/test";
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

            var context = this.GetNancyContext(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                InformationTitle = "{Method} | {Body} | {Query} | {Host} | {Path} | {RequestHeaders} | {Ip} | {IsSuccessful} | {ProtocolVersion} | {StatusCode} | {StatusDescription} | {StatusCodeFamily} | {ErrorException} | {Content} | {ResponseHeaders} | {ElapsedMilliseconds}",
                Blacklist = new string[] { "test" }
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context);

            // assert
            Assert.Contains("[Information] \"POST\" | \"{\r\n  \\\"test\\\": \\\"******\\\",\r\n  \\\"test2\\\": \\\"123\\\"\r\n}\" | \"\" | \"localhost\" | \"/test\" | \"{\r\n  \\\"Content-Type\\\": \\\"application/json\\\",\r\n  \\\"Accept\\\": \\\"application/json,text/xml\\\"\r\n}\" | \"127.0.0.1\" | False | \"1.1\" | 400 | \"BadRequest\" | \"4XX\" | null | \"{ \\\"xpto\\\" : \\\"test\\\" }\" | \"{\r\n  \\\"Content-Type\\\": \\\"application/json\\\"\r\n}\" | \"??\"", this.TestOutputHelper.Output);
        }

        [Fact]
        public void LogData_Should_Work_With_Blacklist_And_Information_With_Default_Title()
        {
            // arrange
            var originIp = "127.0.0.1";
            var protocolVersion = "1.1";

            var requestMethod = "POST";
            var requestBody = "{ \"test\" : \"123\", \"test2\" : \"123\" }";
            var requestUrl = "http://localhost/test";
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

            var context = this.GetNancyContext(
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

            var context = this.GetNancyContext(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var config = new NancySerilogConfiguration
            {
                ErrorTitle = "{Method} | {Body} | [{Path}] | {RequestHeaders} | {Query} | {UrlBase} | {Ip} | {StatusCode} | {StatusCodeFamily} | {ErrorException} | {Content} | {ResponseHeaders} | {ElapsedMilliseconds}",
            };
            var logger = new CommunicationLogger(config);

            // act
            logger.LogData(context, exception);

            // assert
            Assert.Contains("[Error] \"POST\" | \"{ \\\"test\\\" : \\\"123\\\" }\" | [\"/test\"] | \"{\r\n  \\\"Content-Type\\\": \\\"text/html\\\",\r\n  \\\"Accept\\\": \\\"text/html,text/xml\\\"\r\n}\" | \"?sometest=rrtt\" | \"http://some.thing.xp:80\" | \"??\" | 500 | \"5XX\" | \"System.ArgumentNullException: Value cannot be null.\r\nParameter name: test\" | \"\" | \"{\r\n  \\\"Content-Type\\\": \\\"text/html\\\"\r\n}\" | \"??\"", this.TestOutputHelper.Output);
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

            var context = this.GetNancyContext(
                requestMethod, requestUrl, requestBody, requestHeaders,
                responseStatusCode, Content, responseHeaders,
                originIp, protocolVersion);

            var logger = new CommunicationLogger();

            // act
            logger.LogData(context, exception);

            // assert
            Assert.Contains("[Error] HTTP \"POST\" \"/test\" from \"??\" responded 500 in \"??\" ms", this.TestOutputHelper.Output);
        }

        private NancyContext GetNancyContext(string requestMethod, string requestUrl, string requestBody, Dictionary<string, IEnumerable<string>> requestHeaders, HttpStatusCode responseStatusCode, string Content, Dictionary<string, string> responseHeaders, string originIp, string protocolVersion)
        {
            MemoryStream requestBodyStream = (requestBody != null)
                ? new MemoryStream(Encoding.ASCII.GetBytes(requestBody))
                : null ; 

            MemoryStream ContentStream = (Content != null) 
                ? new MemoryStream(Encoding.ASCII.GetBytes(Content))
                : null ;

            var request = new Request(requestMethod, new Url(requestUrl), requestBodyStream, requestHeaders, originIp, null, protocolVersion);
            var response = (Response)Content;
            response.StatusCode = responseStatusCode;
            response.Headers = responseHeaders;

            return new NancyContext
            {
                Request = request,
                Response = response
            };
        }
    }
}
