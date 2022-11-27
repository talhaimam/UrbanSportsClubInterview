using System;
using System.Collections.Generic;
using System.Linq;
using InterviewService.Dto.Output;
using InterviewService.Mappers;
using InterviewService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Controllers
{
    /// <summary>
    /// This API should only be used for synchronously syncing data between services.
    /// </summary>
    [Route("[controller]")]
    [Authorize(Policy = "Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SyncController : BaseController
    {
        public SyncController(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all <see cref="Booking"/>s for all given <paramref name="ids"/>. If resources do not exist for any <paramref name="ids"/>, this will return <see cref="StatusCodes.Status404NotFound"/>.
        /// </summary>
        [Route("bookings")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FitogramMQ.EventGroup[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MessageDto))]
        public ActionResult<IEnumerable<FitogramMQ.Booking>> GetBookings([FromQuery] Guid[] ids)
        {
            Guid[] uniqueIds = ids.Distinct().ToArray();

            IQueryable<Booking> query = base.Context.Bookings
                .Where(x => uniqueIds.Contains(x.Id));

            if (query.Count() != uniqueIds.Count())
            {
                Guid[] foundIds = query
                    .Select(x => x.Id)
                    .ToArray();

                Guid[] missingIds = uniqueIds
                    .Where(x => foundIds.Contains(x) == false)
                    .ToArray();

                return base.NotFoundBookings(ids: missingIds);
            }

            return query.ToFitogramMQModel();
        }
    }
}
