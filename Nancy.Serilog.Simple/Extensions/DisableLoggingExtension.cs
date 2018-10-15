using System;

namespace Nancy.Serilog.Simple.Extensions
{
    /// <summary>
    /// Disable serilog extension
    /// </summary>
    public static class DisableLoggingExtension
    {
        /// <summary>
        /// Context item name | DisableLogging
        /// </summary>
        internal const string ITEM_NAME = "DisableLogging";

        /// <summary>
        /// Disable logging for information
        /// Exception will be logged
        /// </summary>
        /// <param name="module"></param>
        public static void DisableLogging(this NancyModule module)
        {
            module?.Context?.Items.Add(ITEM_NAME, true);
        }
    }
}
