using FitogramMQ;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterviewService.Jobs.FitogramMQConsumers
{
    public class EventConsumer : BaseConsumer
    {
        public EventConsumer(DbContext context) : base(context)
        {
        }

        public static void Start(IFitogramMQClient fitogramMQClient)
        {
            fitogramMQClient.AddBulkConsumer<Event>(EventType.Update, data => BackgroundJob.Enqueue<EventConsumer>(x => x.Process(EventType.Update, data, null)) != null, maxItems: 100, maxDelay: 250);
            fitogramMQClient.AddBulkConsumer<Event>(EventType.Sync, data => BackgroundJob.Enqueue<EventConsumer>(x => x.ProcessLow(EventType.Sync, data, null)) != null, maxItems: 1000, maxDelay: 5000);
        }

        [Queue(Constants.Queues.SyncLow)]
        public void ProcessLow(EventType eventType, IEnumerable<Event> items, PerformContext performContext = null)
        {
            this.Process(eventType, items, performContext);
        }

        [Queue(Constants.Queues.Sync)]
        public void Process(EventType eventType, IEnumerable<Event> items, PerformContext performContext = null)
        {
            try
            {
                this.Context.ChangeTracker.AutoDetectChangesEnabled = false;

                //Write progressbar into the job logs
                IProgressBar progress = performContext?.WriteProgressBar();

                //Load the existing events
                var eventIds = items.Select(x => x.Id).ToArray();
                var existingEvents = this.Context.Events
                    .Where(x => eventIds.Contains(x.Id))
                    .ToList();

                int eventsCreated = 0;
                int eventsUpdated = 0;

                //Prepare a list to do a bulk insert of new entities before save changes
                List<Models.External.Event> newEvents = new List<Models.External.Event>();

                if (performContext != null)
                    items = items.WithProgress(progress);

                //Update the properties of the recived entity
                foreach (var eventData in items)
                {
                    var evnt = existingEvents.FirstOrDefault(x => x.Id == eventData.Id);

                    if (evnt == null)
                    {
                        evnt = new Models.External.Event { Id = eventData.Id, };
                        newEvents.Add(evnt);
                        eventsCreated++;
                    }

                    //Check if we have to update the entity by the timestamp
                    if (evnt.TimeStamp == DateTimeOffset.MinValue || eventData.IsNewerThan(evnt.TimeStamp))
                    {
                        evnt.Start = eventData.Start;
                        evnt.End = eventData.End;
                        evnt.ProviderId = eventData.ProviderId;
                        evnt.TimeStamp = eventData.TimeStamp;
                        evnt.Deleted = eventData.Deleted;

                        eventsUpdated++;
                    }
                }

                //Bulk insert of new events
                this.Context.Events.AddRange(newEvents);

                //Save all changes
                performContext?.WriteLine("Detect changes...");
                this.Context.ChangeTracker.DetectChanges();
                performContext?.WriteLine("Save to database...");
                this.Context.SaveChanges
                (
                    skipBeforeSaveChanges: true,
                    skipAfterSaveChanges: true
                );
                performContext?.WriteLine("Done ;)");
                performContext?.WriteLine("");
                performContext?.WriteLine($"Consumed events: {items.Count()}");
                performContext?.WriteLine($"Created events: {eventsCreated}");
                performContext?.WriteLine($"Updated events: {eventsUpdated}");
                performContext?.WriteLine("");
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
                        Hangfire.BackgroundJob.Enqueue<EventConsumer>(x => x.Process(eventType, new FitogramMQ.Event[] { item }, null));
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
