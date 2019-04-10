using Nancy.Serilog.Simple.Extractors;
using Nancy.Serilog.Simple.Tests.Mock;
using System;
using System.Collections.Generic;
using Xunit;

namespace Nancy.Serilog.Simple.Tests.Extractors
{
    /// <summary>
    /// Nancy context extractor test
    /// </summary>
    public static class NancyContextExtractorTest
    {
        [Fact]
        public static void GetStatusCode_Should_Return_200_OK()
        {
            // arrange
            var context = NancyContextMock.Create(responseStatusCode: HttpStatusCode.OK);
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCode(exception);

            // assert
            Assert.Equal(200, statusCode);
        }

        [Fact]
        public static void GetStatusCode_Should_Return_412_PreconditionFailed()
        {
            // arrange
            var context = NancyContextMock.Create(responseStatusCode: HttpStatusCode.PreconditionFailed);
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCode(exception);

            // assert
            Assert.Equal(412, statusCode);
        }

        [Fact]
        public static void GetStatusCode_Should_Return_0_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCode(exception);

            // assert
            Assert.Equal(0, statusCode);
        }

        [Fact]
        public static void GetStatusCode_Should_Return_0_When_Context_Response_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCode(exception);

            // assert
            Assert.Equal(0, statusCode);
        }

        [Fact]
        public static void GetStatusCode_Should_Return_500_InternalServerError_When_Exception_Is_Not_Null()
        {
            // arrange
            var context = NancyContextMock.Create(responseStatusCode: HttpStatusCode.Accepted);
            Exception exception = new Exception();

            // act
            var statusCode = context.GetStatusCode(exception);

            // assert
            Assert.Equal(500, statusCode);
        }

        [Fact]
        public static void GetStatusCodeFamily_Should_Return_2XX_From_200_OK()
        {
            // arrange
            var context = NancyContextMock.Create(responseStatusCode: HttpStatusCode.OK);
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCodeFamily(exception);

            // assert
            Assert.Equal("2XX", statusCode);
        }

        [Fact]
        public static void GetStatusCodeFamily_Should_Return_4XX_From_412_PreconditionFailed()
        {
            // arrange
            var context = NancyContextMock.Create(responseStatusCode: HttpStatusCode.PreconditionFailed);
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCodeFamily(exception);

            // assert
            Assert.Equal("4XX", statusCode);
        }

        [Fact]
        public static void GetStatusCodeFamily_Should_Return_0XX_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCodeFamily(exception);

            // assert
            Assert.Equal("0XX", statusCode);
        }

        [Fact]
        public static void GetStatusCodeFamily_Should_Return_0XX_When_Context_Response_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();
            Exception exception = null;

            // act
            var statusCode = context.GetStatusCodeFamily(exception);

            // assert
            Assert.Equal("0XX", statusCode);
        }

        [Fact]
        public static void GetStatusCodeFamily_Should_Return_5XX_InternalServerError_When_Exception_Is_Not_Null()
        {
            // arrange
            var context = NancyContextMock.Create(responseStatusCode: HttpStatusCode.Accepted);
            Exception exception = new Exception();

            // act
            var statusCode = context.GetStatusCodeFamily(exception);

            // assert
            Assert.Equal("5XX", statusCode);
        }

        [Fact]
        public static void GetQueryString_Should_Return_Null_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var query = context.GetQueryString();

            // assert
            Assert.Null(query);
        }

        [Fact]
        public static void GetQueryString_Should_Return_Null_When_Context_Request_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var query = context.GetQueryString();

            // assert
            Assert.Null(query);
        }

        [Fact]
        public static void GetQueryString_Should_Return_Empty_When_Context_Request_Query_Is_Empty()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(requestUrl: "http://url-without-query.com");

            // act
            var query = context.GetQueryString();

            // assert
            Assert.Empty(query);
        }

        [Fact]
        public static void GetQueryString_Should_Return_Query_Items()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestUrl: "http://url-without-query.com?test=1&test=2&www=123,456&xxx=test&yyy=&zzz");

            // act
            var query = context.GetQueryString();

            // assert
            Assert.Equal("1,2", query["test"]);
            Assert.Equal("123,456", query["www"]);
            Assert.Equal("", query["yyy"]);
            Assert.Equal("zzz", query["zzz"]);
        }

        [Fact]
        public static void GetRequestHeaders_Should_Return_Null_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var headers = context.GetRequestHeaders();

            // assert
            Assert.Null(headers);
        }

        [Fact]
        public static void GetRequestHeaders_Should_Return_Null_When_Context_Request_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var headers = context.GetRequestHeaders();

            // assert
            Assert.Null(headers);
        }

        [Fact]
        public static void GetRequestHeaders_Should_Return_Empty_When_Context_Request_Headers_Is_Empty()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(requestHeaders: null);

            // act
            var headers = context.GetRequestHeaders();

            // assert
            Assert.Empty(headers);
        }

        [Fact]
        public static void GetRequestHeaders_Should_Return_Headers_Items()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                {
                    { "SomeHeader1", new string[] { "Test1", "Test2" } },
                    { "SomeHeader2", new string[] { "Test3" } },
                    { "SomeHeader3", new string[] { "" } },
                    { "SomeHeader4", null},
                    { "SomeHeader5", new string[] { null } },
                });

            // act
            var headers = context.GetRequestHeaders();

            // assert
            Assert.Equal("Test1,Test2", headers["SomeHeader1"].ToString());
            Assert.Equal("Test3", headers["SomeHeader2"].ToString());
            Assert.Equal("", headers["SomeHeader3"].ToString());
            Assert.Equal("", headers["SomeHeader4"]);
            Assert.Equal("", headers["SomeHeader5"]);
        }

        [Fact]
        public static void GetResponseHeaders_Should_Return_Null_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var headers = context.GetResponseHeaders();

            // assert
            Assert.Null(headers);
        }

        [Fact]
        public static void GetResponseHeaders_Should_Return_Null_When_Context_Response_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var headers = context.GetResponseHeaders();

            // assert
            Assert.Null(headers);
        }

        [Fact]
        public static void GetResponseHeaders_Should_Return_Empty_When_Context_Response_Headers_Is_Null()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(responseHeaders: null);

            // act
            var headers = context.GetResponseHeaders();

            // assert
            Assert.Null(headers);
        }

        [Fact]
        public static void GetResponseHeaders_Should_Return_Response_Items()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "SomeHeader1", "Test1" },
                    { "SomeHeader2", "Test3" },
                    { "SomeHeader3", "" },
                    { "SomeHeader4", null}
                });

            // act
            var headers = context.GetResponseHeaders();

            // assert
            Assert.Equal("Test1", headers["SomeHeader1"]);
            Assert.Equal("Test3", headers["SomeHeader2"]);
            Assert.Equal("", headers["SomeHeader3"]);
            Assert.Null(headers["SomeHeader4"]);
        }

        [Fact]
        public static void GetExecutionTime_Should_Return_Default_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var time = context.GetExecutionTime();

            // assert
            Assert.Equal(-1, Convert.ToInt32(time));
        }

        [Fact]
        public static void GetExecutionTime_Should_Return_Default_When_Context_Request_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var time = context.GetExecutionTime();

            // assert
            Assert.Equal(-1, Convert.ToInt32(time));
        }

        [Fact]
        public static void GetExecutionTime_Should_Return_Default_When_Context_Request_Headers_Is_Empty()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(requestHeaders: null);

            // act
            var time = context.GetExecutionTime();

            // assert
            Assert.Equal(-1, Convert.ToInt32(time));
        }

        [Fact]
        public static void GetExecutionTime_Should_Return_Default_When_Context_Request_Headers_Not_Contains_XInternalTime()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "X-Other-Header", "1230" }
                });

            // act
            var time = context.GetExecutionTime();

            // assert
            Assert.Equal(-1, Convert.ToInt32(time));
        }

        [Fact]
        public static void GetExecutionTime_Should_Return_Default_When_Context_Request_Headers_Contains_Invalid_XInternalTime()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "X-Internal-Time", "invalid" }
                });

            // act
            var time = context.GetExecutionTime();

            // assert
            Assert.Equal(-1, Convert.ToInt32(time));
        }

        [Fact]
        public static void GetExecutionTime_Should_Return_Execution_Time()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "X-Internal-Time", "1230" }
                });

            // act
            var time = context.GetExecutionTime();

            // assert
            Assert.Equal("1230", time.ToString());
        }

        [Fact]
        public static void GetRequestKey_Should_Return_Null_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var requestKey = context.GetRequestKey();

            // assert
            Assert.Null(requestKey);
        }

        [Fact]
        public static void GetRequestKey_Should_Return_Null_When_Context_Request_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var requestKey = context.GetRequestKey();

            // assert
            Assert.Null(requestKey);
        }

        [Fact]
        public static void GetRequestKey_Should_Return_Null_When_Context_Request_Headers_Is_Empty()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(requestHeaders: null);

            // act
            var requestKey = context.GetRequestKey();

            // assert
            Assert.Null(requestKey);
        }

        [Fact]
        public static void GetRequestKey_Should_Return_Null_When_Context_Request_Headers_Not_Contains_RequestKey()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "OtherKey", "b2f6759b-50fa-b7c3-7955-4fee4693482e" }
                });

            // act
            var requestKey = context.GetRequestKey();

            // assert
            Assert.Null(requestKey);
        }

        [Fact]
        public static void GetRequestKey_Should_Return_RequestKey_Value()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "RequestKey", "b2f6759b-50fa-b7c3-7955-4fee4693482e" }
                });

            // act
            var requestKey = context.GetRequestKey();

            // assert
            Assert.Equal("b2f6759b-50fa-b7c3-7955-4fee4693482e", requestKey);
        }

        [Fact]
        public static void GetAccountId_Should_Return_Null_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var accountId = context.GetAccountId();

            // assert
            Assert.Null(accountId);
        }

        [Fact]
        public static void GetAccountId_Should_Return_Null_When_Context_Request_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var accountId = context.GetAccountId();

            // assert
            Assert.Null(accountId);
        }

        [Fact]
        public static void GetAccountId_Should_Return_Null_When_Context_Request_Headers_Is_Empty()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(requestHeaders: null);

            // act
            var accountId = context.GetAccountId();

            // assert
            Assert.Null(accountId);
        }

        [Fact]
        public static void GetAccountId_Should_Return_Null_When_Context_Request_Headers_Not_Contains_AccountId()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "OtherId", "b2f6759b-50fa-b7c3-7955-4fee4693482e" }
                });

            // act
            var accountId = context.GetAccountId();

            // assert
            Assert.Null(accountId);
        }

        [Fact]
        public static void GetAccountId_Should_Return_AccountId_Value()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseHeaders: new Dictionary<string, string>
                {
                    { "AccountId", "b2f6759b-50fa-b7c3-7955-4fee4693482e" }
                });

            // act
            var accountId = context.GetAccountId();

            // assert
            Assert.Equal("b2f6759b-50fa-b7c3-7955-4fee4693482e", accountId);
        }

        [Fact]
        public static void GetIp_Should_Return_Default_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var ip = context.GetIp();

            // assert
            Assert.Equal("??", ip);
        }

        [Fact]
        public static void GetIp_Should_Return_Default_When_Context_Request_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var ip = context.GetIp();

            // assert
            Assert.Equal("??", ip);
        }

        [Fact]
        public static void GetIp_Should_Return_Default_When_Context_Request_Headers_Is_Null_And_Origin_Is_Null()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(requestHeaders: null, originIp: null);

            // act
            var ip = context.GetIp();

            // assert
            Assert.Equal("??", ip);
        }

        [Fact]
        public static void GetIp_Should_Return_Default_When_Context_Request_Headers_Not_Contains_XForwarwedFor_And_Origin_Is_Null()
        {
            // arrange
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                {
                    { "Test", new string[] { "233.233.233.233" } }
                },
                originIp: null);

            // act
            var ip = context.GetIp();

            // assert
            Assert.Equal("??", ip);
        }

        [Fact]
        public static void GetIp_Should_Return_Ip_When_Context_Request_Headers_Contains_XForwarwedFor()
        {
            // arrange
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                {
                    { "X-Forwarded-For", new string[] { "233.233.233.233" } }
                });

            // act
            var ip = context.GetIp();

            // assert
            Assert.Equal("233.233.233.233", ip);
        }

        [Fact]
        public static void GetResponseLength_Should_Return_0_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var length = context.GetResponseLength();

            // assert
            Assert.Equal(0, length);
        }

        [Fact]
        public static void GetResponseLength_Should_Return_0_When_Context_Response_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var length = context.GetResponseLength();

            // assert
            Assert.Equal(0, length);
        }

        [Fact]
        public static void GetResponseLength_Should_Return_0_When_Context_Response_Content_Is_Null()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(responseContent: null);

            // act
            var length = context.GetResponseLength();

            // assert
            Assert.Equal(0, length);
        }

        [Fact]
        public static void GetResponseLength_Should_Return_Length_Of_Content()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(responseContent: "0123456789");

            // act
            var length = context.GetResponseLength();

            // assert
            Assert.Equal(10, length);
        }

        [Fact]
        public static void GetResponseContent_Should_Return_Null_When_Context_Is_Null()
        {
            // arrange
            NancyContext context = null;

            // act
            var content = context.GetResponseContent();

            // assert
            Assert.Null(content);
        }

        [Fact]
        public static void GetResponseContent_Should_Return_Null_When_Context_Response_Is_Null()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var content = context.GetResponseContent();

            // assert
            Assert.Null(content);
        }

        [Fact]
        public static void GetResponseContent_Should_Return_Empty_When_Context_Response_Content_Is_Null()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseContent: null,
                responseHeaders: new Dictionary<string, string>
                { { "Content-Type", "text/plain"} });

            // act
            dynamic content = context.GetResponseContent();

            // assert
            Assert.Empty(content["raw_content"]);
        }

        [Fact]
        public static void GetResponseContent_Should_Return_Value_As_String_When_ContentType_Is_Not_ApplicationJson()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseContent: "0123456789",
                responseHeaders: new Dictionary<string, string>
                { { "Content-Type", "text/plain"} });

            // act
            dynamic content = context.GetResponseContent();

            // assert
            Assert.Equal("0123456789", content["raw_content"]);
        }

        [Fact]
        public static void GetResponseContent_Should_Return_Value_As_Dictionary_When_ContentType_Is_ApplicationJson()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseContent: "{ \"sometest\" : \"somevalue\"}",
                responseHeaders: new Dictionary<string, string>
                { { "Content-Type", "application/json"} });

            // act
            var content = context.GetResponseContent();

            // assert
            var dic = (Dictionary<string, object>)content;
            Assert.NotNull(dic);
            Assert.True(dic.ContainsKey("sometest"));
            Assert.Equal("somevalue", dic["sometest"]);
        }

        [Fact]
        public static void GetResponseContent_Should_Return_Value_As_String_When_ContentType_Is_ApplicationJson_And_Content_Is_An_Invalid_Json()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                responseContent: "{ \"sometest : \"somevalue\"}",
                responseHeaders: new Dictionary<string, string>
                { { "Content-Type", "application/json"} });

            // act
            var content = context.GetResponseContent();

            // assert
            Assert.Equal("{ \"sometest : \"somevalue\"}", content);
        }
       
        [Fact]
        public static void GetRequestBody_Should_Return_Null_When_Context_Is_Null_Without_Blacklist()
        {
            // arrange
            NancyContext context = null;

            // act
            var body = context.GetRequestBody(null);

            // assert
            Assert.Null(body);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Null_When_Context_Response_Is_Null_Without_Blacklist()
        {
            // arrange
            NancyContext context = new NancyContext();

            // act
            var body = context.GetRequestBody(null);

            // assert
            Assert.Null(body);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Empty_When_Context_Response_Content_Is_Null_Without_Blacklist()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestBody: null,
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                { { "Content-Type", new string[] { "text/plain" } } });

            // act
            dynamic body = context.GetRequestBody(null);

            // assert
            Assert.Empty(body["raw_body"]);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Value_As_String_When_ContentType_Is_Not_ApplicationJson_With_Empty_Blacklist()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestBody: "0123456789");

            // act
            dynamic body = context.GetRequestBody(new string[] { });

            // assert
            Assert.Equal("0123456789", body["raw_body"]);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Value_As_Dictionary_When_ContentType_Is_ApplicationJson_With_Empty_Blacklist()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestBody: "{ \"sometest\" : \"somevalue\"}",
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                { { "Content-Type", new string[] { "application/json" } } });

            // act
            var body = context.GetRequestBody(null);

            // assert
            var dic = (Dictionary<string, object>)body;
            Assert.NotNull(dic);
            Assert.True(dic.ContainsKey("sometest"));
            Assert.Equal("somevalue", dic["sometest"]);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Value_As_Dictionary_When_ContentType_Is_ApplicationJson_With_Blacklist()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestBody: "{ \"sometest\" : \"somevalue\", \"sometest2\" : \"somevalue2\"}",
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                { { "Content-Type", new string[] { "application/json" } } });

            // act
            var body = context.GetRequestBody(new string[] { "sometest2" });

            // assert
            var dic = (Dictionary<string, object>)body;
            Assert.NotNull(dic);
            Assert.True(dic.ContainsKey("sometest"));
            Assert.Equal("somevalue", dic["sometest"]);
            Assert.True(dic.ContainsKey("sometest2"));
            Assert.Equal("******", dic["sometest2"]);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Value_As_String_When_ContentType_Is_ApplicationJson_And_Content_Is_An_Invalid_Json()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestBody: "{ \"sometest : \"somevalue\"}",
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                    { { "Content-Type", new string[] { "application/json" } } });

            // act
            var body = context.GetRequestBody(null);

            // assert
            Assert.Equal("{ \"sometest : \"somevalue\"}", body);
        }

        [Fact]
        public static void GetRequestBody_Should_Return_Value_As_Dictionary_When_ContentType_Is_XWwwFormUrlencoded()
        {
            // arrange
            NancyContext context = NancyContextMock.Create(
                requestBody: "sometest=somevalue&sometest2=somevalue2&sometest2=somevalue3",
                requestHeaders: new Dictionary<string, IEnumerable<string>>
                    { { "Content-Type", new string[] { "application/x-www-form-urlencoded" } } });

            // act
            var body = context.GetRequestBody(null);

            // assert
            var dic = (Dictionary<string, object>) body;
            Assert.Equal("somevalue", dic["sometest"]);
            Assert.Equal("somevalue2,somevalue3", dic["sometest2"]);
        }
    }
}
