using System;

namespace InterviewService.Models.External
{
    public abstract class ExternalEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
