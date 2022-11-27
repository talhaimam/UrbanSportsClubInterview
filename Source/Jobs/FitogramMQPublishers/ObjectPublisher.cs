using Microsoft.EntityFrameworkCore;
using FitogramMQ;
using InterviewService.Extensions;
using System.Collections.Generic;
using Hangfire;

namespace InterviewService.Jobs.FitogramMQPublishers
{
    [Queue(Constants.Queues.Sync)]
    public class ObjectPublisher<T> where T : FitogramMQModel
    {
        private readonly IFitogramMQClient FitogramMQClient;

        public ObjectPublisher(IFitogramMQClient fitogramMQClient)
        {
            this.FitogramMQClient = fitogramMQClient;
        }

        public void PublishByState(EntityState state, T item)
        {
            FitogramMQClient.Publish(state.ToFitogramMQEnum(), item);
        }

        public void Publish(EventType eventType, T item)
        {
            FitogramMQClient.Publish(eventType, item);
        }

        public void PublishAll(EventType eventType, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                try
                {
                    this.Publish(eventType, item);
                }
                catch
                {
                    BackgroundJob.Enqueue<ObjectPublisher<T>>(c => c.Publish(eventType, item));
                }
            }
        }
    }
}
