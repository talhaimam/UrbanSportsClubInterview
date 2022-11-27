using InterviewService;
using Xunit;
using Shouldly;
using Microsoft.AspNetCore.Mvc;
using InterviewService.Controllers;
using System;
using InterviewService.Models;
using System.Collections.Generic;
using System.Linq;
using InterviewService.Models.External;

namespace Tests.ControllerTests
{
    public class SyncControllerTests
    {
        [Fact]
        public void GetBookings_200()
        {
            // Arrange

            TestEnvironment testEnvironment = new TestEnvironment();

            Provider provider = testEnvironment.AddProvider();
            Customer customer = testEnvironment.AddCustomer(provider: provider);
            Event evnt = testEnvironment.AddEvent(provider: provider);
            Booking booking1 = testEnvironment.AddBooking(evnt: evnt, customer: customer);
            Booking booking2 = testEnvironment.AddBooking(evnt: evnt, customer: customer);
            Booking booking3 = testEnvironment.AddBooking(evnt: evnt, customer: customer);

            using (DbContext context = testEnvironment.CreateContext())
            {
                SyncController syncController = testEnvironment
                    .GetSyncController(context);

                // Act

                ActionResult<IEnumerable<FitogramMQ.Booking>> result = syncController.GetBookings(new Guid[]
                {
                    booking1.Id,
                    booking2.Id,
                });

                // Assert

                result.Value.Count().ShouldBe(2);
                result.Value.Select(x => x.Id).ShouldContain(booking1.Id);
                result.Value.Select(x => x.Id).ShouldContain(booking2.Id);
                result.Value.Select(x => x.Id).ShouldNotContain(booking3.Id);
            }
        }

        [Fact]
        public void GetBookings_404()
        {
            // Arrange

            TestEnvironment testEnvironment = new TestEnvironment();

            Provider provider = testEnvironment.AddProvider();
            Customer customer = testEnvironment.AddCustomer(provider: provider);
            Event evnt = testEnvironment.AddEvent(provider: provider);
            Booking booking1 = testEnvironment.AddBooking(evnt: evnt, customer: customer);
            Booking booking2 = testEnvironment.AddBooking(evnt: evnt, customer: customer);
            Booking booking3 = testEnvironment.AddBooking(evnt: evnt, customer: customer);

            using (DbContext context = testEnvironment.CreateContext())
            {
                SyncController syncController = testEnvironment
                    .GetSyncController(context);

                Guid missingGuid = Guid.NewGuid();

                // Act

                ActionResult<IEnumerable<FitogramMQ.Booking>> actionResult = syncController.GetBookings(new Guid[]
                {
                    booking1.Id,
                    booking2.Id,
                    missingGuid,
                });

                // Assert

                ObjectResult result = (ObjectResult)actionResult.Result;

                result.StatusCode.ShouldBe(404);
            }
        }
    }
}
