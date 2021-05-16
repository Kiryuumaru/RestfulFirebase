﻿using System;
using System.Net.Http;

namespace RestfulFirebase.Extensions.Http
{
    public interface IHttpClientHandlerFactory
    {
        HttpClientHandler GetHttpClientHandler();
    }
}
