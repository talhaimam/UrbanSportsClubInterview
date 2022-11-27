using System;

namespace InterviewService
{
    public static class Constants
    {
        /// <summary>
        /// Our agreed upon minimum datetime.
        /// </summary>
        /// <remarks>
        /// <para>
        /// MSSQL supports 1753 as the minimum. https://blog.jerrynixon.com/2005/03/earliest-date-held-by-sql-server.html
        /// </para>
        /// <para>
        /// Postgres supports 4713BC (or -infinity). https://stackoverflow.com/a/20342986
        /// </para>
        /// </remarks>
        public static DateTimeOffset MinDateTimeOffset { get; } = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// Queues are case sensitive... Pass auf!
        /// </summary>
        public static class Queues
        {
            public const string Default = "default";
            public const string DefaultLow = "default_low";
            public const string Sync = "sync";
            public const string SyncLow = "sync_low";
        }

        public const string TestUuid = "34a09d7e-054f-4ffa-b690-38b58e9a0b84";
    }
}
