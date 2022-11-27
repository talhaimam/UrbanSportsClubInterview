using System.Collections.Generic;
using System.Linq;
using InterviewService.Dto.Output;
using InterviewService.Models;

namespace InterviewService.Mappers
{
    public static class BookingMapper
    {
        public static BookingDto ToDto(this Booking booking)
        {
            return new Booking[] { booking }.ToDto()[0];
        }

        public static List<BookingDto> ToDto(this IEnumerable<Booking> bookings)
        {
            return bookings.AsQueryable().ToDto();
        }

        public static List<BookingDto> ToDto(this IQueryable<Booking> bookings)
        {
            return bookings.Select(x => new BookingDto
            {
                ProviderId = x.ProviderId,
                EventId = x.EventId,
                CustomerId = x.CustomerId,
                Deleted = x.Deleted,
                TimeStamp = x.TimeStamp,
            })
            .ToList();
        }

        public static FitogramMQ.Booking ToFitogramMQModel(this Booking booking)
        {
            return new Booking[] { booking }.ToFitogramMQModel()[0];
        }

        public static List<FitogramMQ.Booking> ToFitogramMQModel(this IEnumerable<Booking> bookings)
        {
            return bookings.AsQueryable().ToFitogramMQModel();
        }

        public static List<FitogramMQ.Booking> ToFitogramMQModel(this IQueryable<Booking> bookings)
        {
            return bookings.Select(booking => new FitogramMQ.Booking
            {
                Id = booking.Id,
                CustomerId = booking.CustomerId,
                EventId = booking.EventId,
                //ProviderId = booking.ProviderId,
                TimeStamp = booking.TimeStamp,
                Deleted = booking.Deleted,
            })
            .ToList();
        }
    }
}
