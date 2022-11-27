using System;

namespace InterviewService.Client.Data
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
