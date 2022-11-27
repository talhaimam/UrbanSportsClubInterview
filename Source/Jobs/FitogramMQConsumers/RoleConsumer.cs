using FitogramMQ;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InterviewService.Jobs.FitogramMQConsumers
{
    public class RoleConsumer
    {
        private readonly DbContext Context;

        public RoleConsumer(DbContext context)
        {
            this.Context = context;
        }

        public static void Start(IFitogramMQClient fitogramMQClient)
        {
            fitogramMQClient.AddBulkConsumer<FitogramMQ.Role>(EventType.Update, data => BackgroundJob.Enqueue<RoleConsumer>(x => x.Process(EventType.Update, data, null)) != null, maxItems: 10, maxDelay: 500);
            fitogramMQClient.AddBulkConsumer<FitogramMQ.Role>(EventType.Sync, data => BackgroundJob.Enqueue<RoleConsumer>(x => x.Process(EventType.Sync, data, null)) != null, maxItems: 100, maxDelay: 5000);
        }

        public void Process(EventType eventType, IEnumerable<FitogramMQ.Role> items, PerformContext performContext = null)
        {
            try
            {
                foreach (var item in items)
                {
                    var role = this.Context.Roles.IgnoreQueryFilters().FirstOrDefault(x => x.Id == item.Id);

                    // If the entity does not exists we create it
                    if (role == null)
                    {
                        role = new InterviewService.Models.External.Role
                        {
                            Id = item.Id,
                            ProviderId = item.ProviderId,
                            UserId = item.UserId,
                            DeactivationDate = item.DeactivationDate,
                            TimeStamp = item.TimeStamp
                        };
                        this.Context.Roles.Add(role);
                    }

                    //Check if we have to update the entity by the timestamp
                    if (role.TimeStamp == DateTimeOffset.MinValue || item.IsNewerThan(role.TimeStamp))
                    {
                        if (item.Deleted != null)
                        {
                            Context.Roles.Remove(role);
                        }
                        else
                        {
                            role.ProviderId = item.ProviderId;
                            role.UserId = item.UserId;
                            role.TimeStamp = item.TimeStamp;
                            role.DeactivationDate = item.DeactivationDate;
                        }
                    }
                }

                this.Context.SaveChanges
                (
                    skipBeforeSaveChanges: true,
                    skipAfterSaveChanges: true
                );
            }
            catch (Exception e)
            {
                performContext?.SetTextColor(ConsoleTextColor.Red);
                performContext?.WriteLine(e.Message);

                if (items.Count() > 1)
                {
                    performContext?.WriteLine("Failed...");
                    performContext?.WriteLine("Enqueue all items to a single job...");

                    foreach (var item in items)
                    {
                        Hangfire.BackgroundJob.Enqueue<RoleConsumer>(x => x.Process(eventType, new FitogramMQ.Role[] { item }, null));
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
