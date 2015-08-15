using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Temperature.Code
{
    /// <summary>
    /// WebClient с утановленным таймаутом на запрос
    /// </summary>
    class TimeoutedWebClient : WebClient
    {
        private int timeOut = 500;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = timeOut;
            return w;
        }
    }
}