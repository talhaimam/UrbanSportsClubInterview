using System;

namespace InterviewService.Models.External
{
    public class Customer : ExternalEntity
    {
        public Guid ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
