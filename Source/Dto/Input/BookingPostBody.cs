using System;

namespace InterviewService.Dto.Input
{
    public class BookingPostBody
    {
        public Guid CustomerId { get; set; }
        public Guid EventId { get; set; }
    }
}
