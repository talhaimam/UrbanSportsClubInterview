using FitogramMQ;
using Microsoft.EntityFrameworkCore;
using System;

namespace InterviewService.Extensions
{
    public static class EntityStateExtensions
    {
        public static EventType ToFitogramMQEnum(this EntityState state)
        {
            switch (state)
            {
                case EntityState.Added:
                case EntityState.Modified:
                case EntityState.Deleted:
                    return EventType.Update;
                default:
                    throw new Exception("State not handled.");
            }
        }
    }
}
