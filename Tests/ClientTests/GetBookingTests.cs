using InterviewService.Client;
using Shouldly;
using System.Threading.Tasks;
using Tests;
using Xunit;
using System.Net;

namespace InterviewService.Tests.ClientTests
{
    public class GetBookingTests
    {
        [Fact]
        public async Task GetBookingById()
        {
            // [Arrange]

            //Ensure service is running
            TestServer.StartService();

            var testEnvironment = new TestEnvironment();
            var provider = testEnvironment.AddProvider();
            var customer = testEnvironment.AddCustomer(provider: provider);
            var evnt = testEnvironment.AddEvent(provider: provider);
            var booking = testEnvironment.AddBooking(evnt: evnt, customer: customer);
            var role = testEnvironment.AddRoleToProvider(provider.Id, testEnvironment.Configuration.SERVICE_USERID);
            var client = new InterviewServiceClient("http://localhost:5000", testEnvironment.Configuration.SERVICE_USERTOKEN);

            // [Act]

            var response = await client.GetBooking(booking.Id);

            // [Assert]

            response.Successful.ShouldBe(true);
            response.HttpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Value.Id.ShouldBe(booking.Id);
        }
    }
}
