using InterviewService.Dto.Input;
using InterviewService.Dto.Output;
using InterviewService.Helpers;
using InterviewService.Mappers;
using InterviewService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using InterviewService.Models.External;

namespace InterviewService.Controllers
{
    [Route("[controller]")]
    public class BookingsController : BaseController
    {
        public readonly ILogger<BookingsController> Logger;

        public BookingsController(DbContext context, ILogger<BookingsController> logger)
            : base(context)
        {
            this.Logger = logger;
        }

        [HttpPost]
        public ActionResult<BookingDto> CreateBooking([FromBody] BookingPostBody body)
        {
            Customer customer = this.Context.Customers
                .FirstOrDefault(x => x.Id == body.CustomerId);

            if (customer == null)
                return NotFound($"Customer not found for ID {body.CustomerId}.");

            Event evnt = this.Context.Events
                .FirstOrDefault(x => x.Id == body.EventId);

            if (evnt == null)
                return NotFound($"Event not found for ID {body.EventId}.");

            if (!base.HasRole(this.User, customer.ProviderId))
                return Unauthorized();

            string userId = base.GetUUIDFromToken(this.User).ToString();
            DateTime utcNow = SystemDateTime.UtcNow;

            Booking booking = new Booking
            {
                ProviderId = customer.ProviderId,
                Customer = customer,
                Event = evnt,
                CreatedBy = userId,
                ModifiedBy = userId
            };

            this.Logger.LogInformation($"Creating an {nameof(Booking)} with ID {{bookingId}}, triggered by {{userId}}.", booking.Id, userId);

            this.Context.SaveChanges(skipBeforeSaveChanges: false, skipAfterSaveChanges: true);

            return booking.ToDto();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BookingDto> GetBooking(Guid id)
        {
            IQueryable<Booking> query = this.Context.Bookings;

            query = query
                .Where(x => x.Id == id);

            BookingDto bookingDto = query
                .ToDto()
                .FirstOrDefault();

            if (bookingDto == null)
                return base.NotFoundBooking(id: id);

            if (base.HasRole(this.User, bookingDto.ProviderId) == false)
                return base.Unauthorized();

            return bookingDto;
        }

        [HttpGet("/query")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingDto[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<IEnumerable<BookingDto>> GetBookings([FromQuery] BookingGetQuery queryParams)
        {
            // If not admin, only show public event groups.
            if (!base.HasRole(this.User, (Guid)queryParams.ProviderId))
                return base.Unauthorized();

            IQueryable<Booking> query = this.Context.Bookings
                .Where(x => x.ProviderId == queryParams.ProviderId)
                .ToList()
                .AsQueryable();

            if (queryParams.EventIds?.Any() == true)
            {
                query = query
                    .Where(booking => queryParams.EventIds
                        .Any(eventId => eventId == booking.EventId)
                    );
            }

            // Apply pagination.
            query = query
                .OrderBy(x => x.Id)
                .Skip(queryParams.Page * queryParams.Size)
                .Take(queryParams.Size);

            return query.ToDto();
        }
    }
}
