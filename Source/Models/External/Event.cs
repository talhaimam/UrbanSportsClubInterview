using System;

namespace InterviewService.Models.External
{
    public class Event : ExternalEntity
    {
        /// <summary>
        /// Time and date this <see cref="Event"/> starts.
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// Time and date this <see cref="Event"/> ends.
        /// </summary>
        public DateTimeOffset End { get; set; }
        public Guid ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
