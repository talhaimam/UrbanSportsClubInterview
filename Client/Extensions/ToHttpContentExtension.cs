using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace InterviewService.Client.Extensions
{
    internal static class ToHttpContentExtension
    {
        internal static HttpContent ToHttpContent(this object body, JsonSerializerSettings jsonSerializerSettings)
        {
            string json = JsonConvert.SerializeObject(body, jsonSerializerSettings);

            return new StringContent
            (
                content: json,
                encoding: Encoding.UTF8,
                mediaType: "application/json"
            );
        }
    }
}