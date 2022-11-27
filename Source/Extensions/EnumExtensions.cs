using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterviewService.Extensions
{
    public static class EnumExtensions
    {
        public static LogEventLevel? ToSerilogEventLevel(this LogLevel value)
        {
            switch (value)
            {
                case LogLevel.Trace:
                    return LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Critical:
                    return LogEventLevel.Fatal;
                case LogLevel.None:
                    throw new NotSupportedException("LogLevel None is not supported. Some level of logging must be enabled.");
                default:
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
                    throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
            }
        }
    }
}
