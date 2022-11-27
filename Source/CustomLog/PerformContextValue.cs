using Hangfire.Server;
using Serilog.Events;
using System;
using System.IO;

namespace InterviewService.CustomLog
{
    internal class PerformContextValue : ScalarValue
    {
        public PerformContextValue(object value) : base(value)
        {
        }

        /// <summary>
        /// Context attached to this property value.
        /// </summary>
        public PerformContext PerformContext { get; set; }

        /// <inheritdoc />
        public override void Render(TextWriter output, string format = null, IFormatProvider formatProvider = null)
        {
            // How the value will be rendered in Json output, etc.
            // Not important for the function of this code..
            output.Write(PerformContext.BackgroundJob.Id);
        }
    }
}
