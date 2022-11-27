using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
namespace InterviewService.Helpers
{
    public class ConfigurationValidationStartupFilter : IStartupFilter
    {
        private readonly IEnumerable<IValidator> _validatableObjects;

        public ConfigurationValidationStartupFilter(IEnumerable<IValidator> validatableObjects)
        {
            _validatableObjects = validatableObjects;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            foreach (var validatableObject in _validatableObjects)
            {
                validatableObject.Validate();
            }

            //don't alter the configuration
            return next;
        }
    }
}
