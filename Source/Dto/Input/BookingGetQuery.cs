using System;
using System.ComponentModel.DataAnnotations;

namespace InterviewService.Dto.Input
{
    public class BookingGetQuery : PaginatedQuery
    {
        [Required]
        public Guid ProviderId { get; set; }

        public Guid[] EventIds { get; set; }
    }
}
