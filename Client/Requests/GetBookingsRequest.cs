using System;

namespace InterviewService.Client.Requests
{
    public class GetBookingsRequest
    {
        public Guid ProviderId { get; set; }
        public Guid[] EventIds { get; set; }
    }
}
