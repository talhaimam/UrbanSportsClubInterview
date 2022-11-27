using System;

namespace InterviewService.Models.External
{
    public class Role : ExternalEntity
    {
        public Guid ProviderId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset? DeactivationDate { get; set; }
    }
}
