using System;

namespace GCloud.Shared
{
    public static class BaseUrlContainer
    {
#if DEBUG
        //public const string BaseUrlScheme = "https";
        //public const string BaseUrlHost = "qm.foodjet.online";
        //public const int BaseUrlPort = 443;
        ////public const string BaseUrlScheme = "http";
        ////public const string BaseUrlHost = "192.168.2.213";
        ////public const int BaseUrlPort = 8081;
        public const string BaseUrlScheme = "http";
        public const string BaseUrlHost = "schnitzeltest.foodjet.online";
        public const int BaseUrlPort = 80;
#else
        //public const string BaseUrlScheme = "https";
        //public const string BaseUrlHost = "app.foodjet.online";
        //public const int BaseUrlPort = 443;
        public const string BaseUrlScheme = "http";
        public const string BaseUrlHost = "schnitzeltest.foodjet.online";
        public const int BaseUrlPort = 80;
#endif

        public static Uri BaseUri => new UriBuilder(BaseUrlScheme, BaseUrlHost, BaseUrlPort).Uri;
    }
}
