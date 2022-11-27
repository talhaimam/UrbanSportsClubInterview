using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace InterviewService.Client.Responses
{
    public class Response
    {
        internal Response() { }

        public HttpResponseMessage HttpResponse { get; internal set; }
        public bool Successful { get; internal set; }
        internal string Content { get; set; }

        public override string ToString() => this.Content;
    }

    public class Response<T> : Response where T : class
    {
        internal Response() { }

        public T Value { get; internal set; }
    }
}
