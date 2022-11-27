using InterviewService.Client.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InterviewService.Client.Extensions
{
    internal static class ToResponseExtension
    {
        internal static async Task<Response<T>> ToResponse<T>(this HttpResponseMessage httpResponse, JsonSerializerSettings jsonSerializerSettings) where T : class
        {
            try
            {
                string content = await httpResponse.Content.ReadAsStringAsync();

                T result = httpResponse.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<T>(content, jsonSerializerSettings)
                    : null;

                return new Response<T>
                {
                    HttpResponse = httpResponse,
                    Successful = httpResponse.IsSuccessStatusCode,
                    Value = result,
                    Content = content
                };
            }
            catch (Exception ex)
            {
                return new Response<T>
                {
                    Successful = false,
                    Content = ex.Message
                };
            }
        }

        internal static Response ToResponse(this HttpResponseMessage httpResponse)
        {
            return new Response
            {
                HttpResponse = httpResponse,
                Successful = httpResponse.IsSuccessStatusCode
            };
        }
    }
}
