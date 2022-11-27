using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterviewService.Jobs.FitogramMQConsumers
{
    [Queue(Constants.Queues.Sync)]
    public class BaseConsumer
    {
        protected readonly DbContext Context;

        public BaseConsumer(DbContext context)
        {
            this.Context = context;
        }
    }
}
