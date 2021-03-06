﻿
using Serilog;

namespace Nancy.Serilog.Simple
{
    public class NancySerilogConfiguration
    {
        public string[] Blacklist { get; set; }

        public string InformationTitle { get; set; }

        public string ErrorTitle { get; set; }

        public string Version { get; set; }

        public ILogger Logger { get; set; }
    }
}