using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FitogramMQ;
using InterviewService.Jobs.FitogramMQConsumers;
using InterviewService.Jobs.FitogramMQPublishers;
using InterviewService.Models;
using InterviewService.Models.External;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Tests.JobsTests.FitogramMQConsumersTests
{
    public class ObjectPublisherTests
    {
        [Fact]
        public void TestPublish()
        {
            // [Given]
            var mockedMQClient = new Mock<FitogramMQ.IFitogramMQClient>();

            var publisher = new ObjectPublisher<FitogramMQ.Booking>(mockedMQClient.Object);
            var booking1 = new FitogramMQ.Booking { Id = Guid.NewGuid() };
            var booking2 = new FitogramMQ.Booking { Id = Guid.NewGuid() };

            // [When]
            publisher.PublishAll(EventType.Update, new[] { booking1, booking2 });

            // [Then]
            mockedMQClient.Verify(x => x.Publish(EventType.Update, booking1));
            mockedMQClient.Verify(x => x.Publish(EventType.Update, booking2));
        }
    }
}