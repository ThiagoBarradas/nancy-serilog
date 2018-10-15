using System;
using System.Collections.Generic;

namespace Nancy.Serilog.Simple.Tests.Mock
{
    public class CommunicationLoggerMock : ICommunicationLogger
    {
        public List<NancyContext> Logs { get; set; } = new List<NancyContext>();

        public NancySerilogConfiguration NancySerilogConfiguration { get; set; }

        public void LogData(NancyContext context)
        {
            context.Items["WorksWithoutException"] = true;
            Logs.Add(context);
        }

        public void LogData(NancyContext context, Exception exception)
        {
            context.Items["WorksWithException"] = true;
            context.Items["Exception"] = exception;
            Logs.Add(context);
        }
    }
}
