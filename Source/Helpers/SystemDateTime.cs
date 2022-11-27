using System;

namespace InterviewService.Helpers
{
    public static class SystemDateTime
    {
        [ThreadStatic]
        private static DateTime? FixedDateTime;

        public static DateTime UtcNow
        {
            get => FixedDateTime ?? DateTime.UtcNow;
            set => FixedDateTime = value;
        }

        /// <summary>
        /// Should only be used for testing.
        /// </summary>
        public static void SetConstant()
        {
            UtcNow = UtcNow;
        }
    }
}
