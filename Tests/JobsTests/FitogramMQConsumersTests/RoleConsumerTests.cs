using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using InterviewService.Jobs.FitogramMQConsumers;
using InterviewService.Models;
using InterviewService.Models.External;

namespace Tests.JobsTests.FitogramMQConsumersTests
{
    public class RoleConsumerTests
    {
        [Fact]
        public void GvenRoleIsProcessedMakeSureItsSaved()
        {
            var testEnvironment = new TestEnvironment();

            using (TransactionScope scope = new TransactionScope())
            using (var context = testEnvironment.CreateContext())
            {
                var roleConsumer = new RoleConsumer(context: context);

                context.Roles.RemoveRange(context.Roles.ToList());
                context.SaveChanges();

                var offset = new DateTimeOffset(2020, 02, 02, 01, 01, 0, 1, TimeSpan.FromHours(1));
                var userId = Guid.NewGuid();
                var providerId = Guid.NewGuid();

                FitogramMQ.Role[] rolesToBeConsumed = new[]
                {
                        new FitogramMQ.Role
                        {
                            Id = Guid.NewGuid(),
                            Name = "FullAccess",
                            Deleted = null,
                            Description = "Desc",
                            InvitationSent = null,
                            EndDateTime= null,
                            UserId = userId,
                            ProviderId = providerId,
                            TimeStamp = offset
                        },
                    };

                roleConsumer.Process(eventType: FitogramMQ.EventType.Update, items: rolesToBeConsumed);

                Guid[] ids = rolesToBeConsumed.Select(d => d.Id).ToArray();

                List<Role> consumeRoles = context.Roles
                    .Where(e => ids.Contains(e.Id))
                    .ToList();

                Assert.Equal(rolesToBeConsumed.Length, consumeRoles.Count);

                foreach (Role consumedRole in consumeRoles)
                {
                    FitogramMQ.Role roleToBeConsumed = rolesToBeConsumed
                        .First(e => e.Id == consumedRole.Id);

                    Assert.Equal(roleToBeConsumed.Id, consumedRole.Id);
                    Assert.Equal(roleToBeConsumed.Deleted, consumedRole.Deleted);
                    Assert.Equal(roleToBeConsumed.UserId, consumedRole.UserId);
                    Assert.Equal(roleToBeConsumed.ProviderId, consumedRole.ProviderId);
                    Assert.Equal(roleToBeConsumed.TimeStamp, consumedRole.TimeStamp);
                }
            }
        }
    }
}
