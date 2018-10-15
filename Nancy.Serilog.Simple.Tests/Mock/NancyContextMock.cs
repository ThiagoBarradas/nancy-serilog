using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nancy.Serilog.Simple.Tests.Mock
{
    /// <summary>
    /// NancyContext Mock
    /// </summary>
    internal static class NancyContextMock
    {
        /// <summary>
        /// Create new nancy context
        /// </summary>
        /// <param name="requestMethod"></param>
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="responseStatusCode"></param>
        /// <param name="responseContent"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="originIp"></param>
        /// <param name="protocolVersion"></param>
        /// <returns></returns>
        internal static NancyContext Create(
            string requestMethod = "GET", 
            string requestUrl = "http://localhost.com",
            string requestBody = null, 
            IDictionary<string, IEnumerable<string>> requestHeaders = null,
            HttpStatusCode responseStatusCode = HttpStatusCode.OK, 
            string responseContent = null, 
            IDictionary<string, string> responseHeaders = null, 
            string originIp = "127.0.0.1", 
            string protocolVersion = "1.1")
        {
            MemoryStream requestBodyStream = (requestBody != null)
                ? new MemoryStream(Encoding.ASCII.GetBytes(requestBody))
                : null;

            var request = new Request(requestMethod, new Url(requestUrl), requestBodyStream, requestHeaders, originIp, null, protocolVersion);

            var response = (Response)responseContent;
            response.StatusCode = responseStatusCode;
            response.Headers = responseHeaders;

            if (responseHeaders != null && responseHeaders.ContainsKey("Content-Type") && string.IsNullOrWhiteSpace(responseHeaders["Content-Type"]) == false)
            {
                response.ContentType = responseHeaders["Content-Type"];
            }

            return new NancyContext
            {
                Request = request,
                Response = response
            };
        }
    }
}
