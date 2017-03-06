using System;
using System.Net;

namespace tcp_moe_server.Classes
{
    public class Http
    {
        private WebClient wc = new WebClient();

        public delegate void HttpResultCallback(string result);
        HttpResultCallback callback;

        public Http(string url, string parameters, HttpResultCallback callback)
        {
            this.callback = callback;

            wc.DownloadStringCompleted += Wc_DownloadStringCompleted;
            wc.DownloadStringAsync(new Uri(Config.apiUrl + url + parameters));
        }
        private void Wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            callback(e.Result);
        }
    }
}
