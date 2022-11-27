using InterviewService.Models;
using InterviewService.Models.External;
using Xunit;
using Shouldly;
using Microsoft.AspNetCore.Mvc;
using InterviewService.Dto.Output;

namespace Tests.ControllerTests
{
    public class BookingControllerTests
    {
        [Fact]
        public void GetBooking_ById_Exists()
        {
            // [Arrange]

            TestEnvironment testEnvironment = new TestEnvironment();

            Provider provider = testEnvironment.AddProvider();
            Customer customer = testEnvironment.AddCustomer(provider: provider);
            Event evnt = testEnvironment.AddEvent(provider: provider);
            Booking booking = testEnvironment.AddBooking(evnt: evnt, customer: customer);
            testEnvironment.AddRoleToProvider(provider.Id);

            // [Act]

            ActionResult<BookingDto> existingBooking = testEnvironment
                .GetBookingsController()
                .GetBooking(id: booking.Id);

            existingBooking.ShouldNotBeNull();

            BookingDto bookingDto = existingBooking.Value;

            // [Assert]

            bookingDto.ShouldNotBe(null);
            bookingDto.Id.ShouldBe(booking.Id);
        }
    }
}
