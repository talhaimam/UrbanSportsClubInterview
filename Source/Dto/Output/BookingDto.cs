using System;

namespace InterviewService.Dto.Output
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
