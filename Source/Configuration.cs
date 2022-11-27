using InterviewService.Helpers;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace InterviewService
{
    public class Configuration : IValidator
    {
        [Required]
        public string ENVIRONMENT { get; set; }

        [Required]
        public string SERVICENAME { get; set; }

        [Required]
        public string DB_CONNECTION_STRING { get; set; }

        [Required]
        public string HANGFIRE_REDIS_ENDPOINT { get; set; }

        public bool RABBITMQ_DISABLE { get; set; }

        [Required]
        public string RABBITMQ_ENDPOINT { get; set; }

        [Required]
        public string RABBITMQ_PORT { get; set; }

        [Required]
        public string RABBITMQ_USERNAME { get; set; }

        [Required]
        public string RABBITMQ_PASSWORD { get; set; }

        [Required]
        public string JWT_TOKEN { get; set; }

        public string SERVICE_USERTOKEN { get; set; }
        public string SERVICE_USERID { get; set; }

        /// <summary>
        /// Main application logging.
        /// </summary>
        [Required]
        public LogLevel LOG_LEVEL { get; set; }

        /// <summary>
        /// For Microsoft and System (etc) namespace logging. Not main application logging.
        /// </summary>
        public LogLevel? LOG_LEVEL_EXTRA { get; set; }

        /// <summary>
        /// If Hangfire jobs should be processed.
        /// </summary>
        public bool RUN_HANGFIRE_JOBS { get; set; } = true;

        /// <summary>
        /// If recurring Hangfire jobs should be enqueued.
        /// </summary>
        public bool RUN_RECURRING_JOBS { get; set; }

        public void Validate()
        {
            Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
        }
    }
}
