using System;
using System.ComponentModel.DataAnnotations;

namespace InterviewService.Dto.Input
{
    public class PaginatedQuery
    {
        /// <summary>
        /// Current page of results.
        /// </summary>
        [Range(0, Int32.MaxValue)]
        public int Page { get; set; }

        /// <summary>
        /// Number of results per a page.
        /// </summary>
        [Range(1, 10000)]
        public int Size { get; set; } = 10000;
    }
}
