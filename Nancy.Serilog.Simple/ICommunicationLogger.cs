using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using System;

namespace Nancy.Serilog.Simple
{
    /// <summary>
    /// Communication Logger interface
    /// </summary>
    public interface ICommunicationLogger
    {
        /// <summary>
        /// Nancy Serilog Configuration
        /// </summary>
        NancySerilogConfiguration NancySerilogConfiguration { get; set; }

        /// <summary>
        /// Log context
        /// </summary>
        /// <param name="context"></param>
        void LogData(NancyContext context);

        /// <summary>
        /// Log context and exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void LogData(NancyContext context, Exception exception);
    }
}
