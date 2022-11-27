using System;
using Xunit;
using Shouldly;
using System.Linq;
using InterviewService.Jobs.FitogramMQConsumers;
using System.Collections.Generic;

namespace Tests.JobsTests.FitogramMQConsumersTests
{
    public class ProviderConsumerTests
    {
        [Fact]
        public void ConsumeEventBeforeRelatedData()
        {
            var testEnvironment = new TestEnvironment();
            var consumer = new ProviderConsumer(testEnvironment.CreateContext());
            var data = new List<FitogramMQ.Models.ServiceProvider>  {
               new FitogramMQ.Models.ServiceProvider
               {
                   Id = Guid.NewGuid(),
                   OwnerUuids  = new Guid[] {Guid.NewGuid()}
               }
           };

            consumer.Process(FitogramMQ.EventType.Sync, data);

            using (InterviewService.DbContext context = testEnvironment.CreateContext())
            {
                var provider = context.Providers
                    .FirstOrDefault(x => x.Id == data.First().Id);

                provider.ShouldNotBeNull();
                provider.Id.ShouldBe(data[0].Id);
            }
        }
    }
}