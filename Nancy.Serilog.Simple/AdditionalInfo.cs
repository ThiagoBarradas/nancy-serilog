using System.Collections.Generic;

namespace Nancy.Serilog.Simple
{
    public class AdditionalInfo
    {
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
