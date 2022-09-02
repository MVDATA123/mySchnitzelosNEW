using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GCloudShared.Shared
{
    public class MyHttpClientHandler : HttpClientHandler
    {
        public IInternetState InternetState { get; set; }
        
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (InternetState != null && InternetState.State == NetworkState.Disconnected)
            {
                InternetState.ShowNoInternetMessage();
                return Task.FromResult(new HttpResponseMessage(0));
            }
            return base.SendAsync(request, cancellationToken).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    InternetState.AuthState = AuthState.Unauthorized;
                    InternetState.LogoffRedirectToLogin();
                    return new HttpResponseMessage(0);
                }

                return t.Result;
            }, cancellationToken);
        }
    }
}