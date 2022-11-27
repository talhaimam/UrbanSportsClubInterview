using InterviewService.Client.Data;
using InterviewService.Client.Extensions;
using InterviewService.Client.Requests;
using InterviewService.Client.Responses;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace InterviewService.Client
{
    public class InterviewServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public InterviewServiceClient(string serviceBaseUrl, string authenticationToken = null)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(serviceBaseUrl);

            if (authenticationToken != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);
            }

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync(3);

            _jsonSerializerSettings = new JsonSerializerSettings();
            _jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        /// <summary>
        /// Get a booking from [GET /bookings/{id}]
        /// </summary>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<Response<Booking>> GetBooking(Guid bookingId)
        {
            HttpResponseMessage responseMessage = await _retryPolicy
                .ExecuteAsync(() => _httpClient.GetAsync("/bookings/" + bookingId.ToString()));

            return await responseMessage.ToResponse<Booking>(_jsonSerializerSettings);
        }

        /// <summary>
        /// Get bookings from [GET /bookings]
        /// </summary>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<Response<Booking[]>> GetBookings(GetBookingsRequest query)
        {
            HttpResponseMessage responseMessage = await _retryPolicy
                .ExecuteAsync(() => _httpClient.GetAsync("/bookings" + query.ToQueryString()));

            return await responseMessage.ToResponse<Booking[]>(_jsonSerializerSettings);
        }

        /// <summary>
        /// Checks if the service is healthy
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckHealth()
        {
            HttpResponseMessage responseMessage = await _retryPolicy
                .ExecuteAsync(() => _httpClient.GetAsync("/healthcheck"));

            return responseMessage.IsSuccessStatusCode;
        }
    }
}
