/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

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

            try
            {
                wc.DownloadStringAsync(new Uri(Config.apiUrl + url + parameters));
            }
            catch { }
        }
        private void Wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                callback(e.Result);
            }
            catch { }
        }
    }
}
