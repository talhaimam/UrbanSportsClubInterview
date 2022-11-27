using InterviewService.Helpers;
using InterviewService.Jobs.FitogramMQPublishers;
using InterviewService.Mappers;
using Fitogram.EntityFrameworkCore;
using Hangfire;
using System;
using InterviewService.Models.External;

namespace InterviewService.Models
{
    public class Booking : PublicEntity, IBeforeSaveChanges, IAfterSaveChanges
    {
        public Guid ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public void BeforeSaveChanges(EntityUpdate update)
        {
            if (this.TimeStamp == DateTimeOffset.MinValue)
                this.CreatedDate = SystemDateTime.UtcNow;

            this.TimeStamp = SystemDateTime.UtcNow;
            this.ModifiedDate = SystemDateTime.UtcNow;
        }

        public void AfterSaveChanges(EntityUpdate update)
        {
            BackgroundJob.Enqueue<ObjectPublisher<FitogramMQ.Booking>>(c => c.PublishByState(update.EntityState, this.ToFitogramMQModel()));
        }
    }
}
