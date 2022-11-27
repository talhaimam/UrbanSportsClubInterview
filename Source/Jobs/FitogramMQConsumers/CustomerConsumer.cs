using FitogramMQ;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterviewService.Jobs.FitogramMQConsumers
{
    public class CustomerConsumer
    {
        private readonly DbContext Context;

        public CustomerConsumer(DbContext context)
        {
            this.Context = context;
        }

        public static void Start(IFitogramMQClient fitogramMQClient)
        {
            fitogramMQClient.AddBulkConsumer<Customer>(EventType.Update, data => BackgroundJob.Enqueue<CustomerConsumer>(x => x.Process(EventType.Update, data, null)) != null, maxItems: 10, maxDelay: 500);
            fitogramMQClient.AddBulkConsumer<Customer>(EventType.Sync, data => BackgroundJob.Enqueue<CustomerConsumer>(x => x.Process(EventType.Sync, data, null)) != null, maxItems: 100, maxDelay: 5000);
        }

        public void Process(EventType eventType, IEnumerable<Customer> items, PerformContext performContext = null)
        {
            try
            {
                foreach (var item in items)
                {
                    var customer = this.Context.Customers.FirstOrDefault(x => x.Id == item.Id);

                    // If the entity does not exists we create it
                    if (customer == null)
                    {
                        customer = new Models.External.Customer { Id = item.Id };
                        this.Context.Customers.Add(customer);
                    }

                    // Check if we have to update the entity by the timestamp
                    if (customer.TimeStamp == DateTimeOffset.MinValue || item.IsNewerThan(customer.TimeStamp))
                    {
                        customer.TimeStamp = item.TimeStamp;
                        customer.Deleted = item.Deleted;
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
                        Hangfire.BackgroundJob.Enqueue<CustomerConsumer>(x => x.Process(eventType, new Customer[] { item }, null));
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
