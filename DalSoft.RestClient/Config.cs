﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;

namespace DalSoft.RestClient
{
    public class Config
    {
        internal static readonly string JsonMediaType = "application/json";
        internal static readonly string RequestContentKey = "DalSoft.RestClient.RequestContentKey";
        internal static readonly string RequestContentType = "DalSoft.RestClient.RequestContentType";
        internal static readonly string ResponseIsJsonKey = "DalSoft.RestClient.ResponseIsJsonKey";

        internal IEnumerable<HttpMessageHandler> Pipeline { get; set; }

        public TimeSpan Timeout { get; set; }
        public long MaxResponseContentBufferSize { get; set; }
        public bool UseDefaultHandlers { get; set; }
        
        public Config() : this((HttpMessageHandler[])null) { }

        public Config(params Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>>[] handlers) :
            this(handlers?.Select(handler => new DelegatingHandlerWrapper(handler)).ToArray()) {  }

        public Config(params HttpMessageHandler[] pipeline)
        {
            pipeline = pipeline ?? new HttpMessageHandler[] {};
            pipeline.ValidatePipeline();

            var handlers = pipeline.ToList();
            handlers.Insert(0, new DefaultJsonHandler(this));

            Pipeline = handlers;
            Timeout = TimeSpan.FromSeconds(100.0);        //Same default as HttpClient
            MaxResponseContentBufferSize = int.MaxValue;  //Same default as HttpClient
            UseDefaultHandlers = true;
        }
    }   
}
