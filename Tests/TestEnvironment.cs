using InterviewService;
using InterviewService.Controllers;
using InterviewService.Helpers;
using InterviewService.Models;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Configuration;
using InterviewService.Tests;
using Microsoft.Extensions.Logging;
using InterviewService.Models.External;

namespace Tests
{
    public class TestEnvironment
    {
        private DbContextOptions<InterviewService.DbContext> DbContextOptions { get; }
        public Configuration Configuration { get; set; }
        public ClaimsPrincipal TestUser { get; set; }

        /// <summary>
        /// Create a logger that forwards log messages to the console.
        /// </summary>
        public ILogger<T> CreateLogger<T>()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
            return loggerFactory.CreateLogger<T>();
        }

        static TestEnvironment()
        {
            var context = new TestEnvironment().CreateContext();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        public TestEnvironment()
        {
            this.Configuration = this.LoadConfiguration();

            this.DbContextOptions = new DbContextOptionsBuilder<InterviewService.DbContext>()
                .UseLazyLoadingProxies(true)
                .UseNpgsql(this.Configuration.DB_CONNECTION_STRING)
                //.UseInMemoryDatabase(databaseName: databaseName) // Can't use in memory db becuase it does not support array properties (Guid[]).
                .Options;

            GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseBatches();

            #region fake user
            //Let's fake a claim for an admin user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim("user_type", "pro"),
                new Claim("user_uuid", Constants.TestUuid),
                new Claim("sub", Guid.NewGuid().ToString())
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims);
            this.TestUser = new ClaimsPrincipal(identity);
            #endregion fake user
        }

        #region SetCurrentTime
        public void SetCurrentTime(in DateTime newTime)
        {
            SystemDateTime.UtcNow = newTime;
        }
        #endregion SetCurrentTime

        #region CreateContext
        public InterviewService.DbContext CreateContext() => new InterviewService.DbContext(this.DbContextOptions, this.Configuration);
        #endregion CreateContext

        public Int32 CreateNewIntegerId()
            => new Random().Next(0, Int32.MaxValue);

        public string CreateNewVirtualId()
            => "V1234-" + new Random().Next(0, Int32.MaxValue);

        #region AddProvider
        public Provider AddProvider(Guid? id = null)
        {
            id = id ?? Guid.NewGuid();

            using (InterviewService.DbContext context = this.CreateContext())
            {
                var providerEntity = context.Providers.Add(new Provider
                {
                    Id = id.Value,
                });

                context.SaveChanges
                (
                    skipBeforeSaveChanges: true,
                    skipAfterSaveChanges: true
                );

                return providerEntity.Entity;
            }
        }
        #endregion AddProvider
        #region AddRoleToProvider
        public Role AddRoleToProvider(Guid providerId, string userId = Constants.TestUuid)
        {
            using (InterviewService.DbContext context = this.CreateContext())
            {
                var roleEntity = context.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    ProviderId = providerId,
                    UserId = Guid.Parse(userId)
                });

                context.SaveChanges
                (
                    skipBeforeSaveChanges: true,
                    skipAfterSaveChanges: true
                );

                return roleEntity.Entity;
            }
        }
        #endregion AddRoleToProvider

        #region AddCustomer
        public Customer AddCustomer(Provider provider)
        {
            using (InterviewService.DbContext context = this.CreateContext())
            {
                var customer = context.Customers.Add(new Customer
                {
                    ProviderId = provider.Id,
                });

                context.SaveChanges();

                return customer.Entity;
            }
        }
        #endregion AddCustomer

        #region AddEvent
        public Event AddEvent(Provider provider)
        {
            using (InterviewService.DbContext context = this.CreateContext())
            {
                var evnt = context.Events.Add(new Event
                {
                    ProviderId = provider.Id,
                });

                context.SaveChanges();

                return evnt.Entity;
            }
        }
        #endregion AddEvent

        #region AddBooking
        public Booking AddBooking(Event evnt, Customer customer)
        {
            using (InterviewService.DbContext context = this.CreateContext())
            {
                if (evnt != null)
                    evnt = context.Events.First(x => x.Id == evnt.Id);

                if (customer != null)
                    customer = context.Customers.First(x => x.Id == customer.Id);

                var booking = context.Bookings.Add(new Booking
                {
                    ProviderId = evnt.Provider.Id,
                    EventId = evnt.Id,
                    CustomerId = customer.Id,
                });

                context.SaveChanges();

                return booking.Entity;
            }
        }
        #endregion AddBooking

        #region GetEventController
        public BookingsController GetBookingsController(InterviewService.DbContext context = null)
        {
            context = context ?? this.CreateContext();

            return new BookingsController(context: context, logger: this.CreateLogger<BookingsController>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = this.TestUser
                    }
                }
            };
        }
        #endregion GetEventController

        #region GetSyncController
        public SyncController GetSyncController(InterviewService.DbContext context = null)
        {
            context = context ?? this.CreateContext();

            return new SyncController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = this.TestUser
                    }
                }
            };
        }
        #endregion GetSyncController

        #region LoadConfiguration
        private Configuration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .Build()
                .Get<Configuration>();
        }
        #endregion LoadConfiguration
    }
}
