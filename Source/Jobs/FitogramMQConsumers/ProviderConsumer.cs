using FitogramMQ;
using FitogramMQ.Models;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterviewService.Jobs.FitogramMQConsumers
{
    public class ProviderConsumer
    {
        private readonly DbContext Context;

        public ProviderConsumer(DbContext context)
        {
            this.Context = context;
        }

        public static void Start(IFitogramMQClient fitogramMQClient)
        {
            fitogramMQClient.AddBulkConsumer<ServiceProvider>(EventType.Update, data => BackgroundJob.Enqueue<ProviderConsumer>(x => x.Process(EventType.Update, data, null)) != null, maxItems: 10, maxDelay: 500);
            fitogramMQClient.AddBulkConsumer<ServiceProvider>(EventType.Sync, data => BackgroundJob.Enqueue<ProviderConsumer>(x => x.Process(EventType.Sync, data, null)) != null, maxItems: 100, maxDelay: 5000);
        }

        public void Process(EventType eventType, IEnumerable<ServiceProvider> items, PerformContext performContext = null)
        {
            try
            {
                foreach (var item in items)
                {
                    var provider = this.Context.Providers.FirstOrDefault(x => x.Id == item.UUID);

                    // If the entity does not exists we create it
                    if (provider == null)
                    {
                        provider = new Models.External.Provider { Id = item.Id };
                        this.Context.Providers.Add(provider);
                    }

                    // Check if we have to update the entity by the timestamp
                    if (provider.TimeStamp == DateTimeOffset.MinValue || item.IsNewerThan(provider.TimeStamp))
                    {
                        provider.TimeStamp = item.TimeStamp;
                        provider.Deleted = item.Deleted;
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
                        Hangfire.BackgroundJob.Enqueue<ProviderConsumer>(x => x.Process(eventType, new ServiceProvider[] { item }, null));
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
