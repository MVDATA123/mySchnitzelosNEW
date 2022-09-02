using System;
using System.Collections.Generic;
using System.Text;

namespace GCloudShared.Shared
{
    public sealed class UriContainer
    {
        public static readonly UriContainer BasePath = new UriContainer(GCloud.Shared.BaseUrlContainer.BaseUrlScheme, GCloud.Shared.BaseUrlContainer.BaseUrlHost, GCloud.Shared.BaseUrlContainer.BaseUrlPort);

        private string Host { get; set; }
        private string Schema { get; set; }
        private int Port { get; set; }
        public string Path { get; set; }

        public UriContainer(string schema, string host, int port)
        {
            Host = host;
            Schema = schema;
            Port = port;
        }

        public UriContainer(string schema, string host, int port, string path)
        {
            Host = host;
            Schema = schema;
            Port = port;
            Path = path;
        }

        public UriContainer(UriContainer basePath, string path)
        {
            Host = basePath.Host;
            Port = basePath.Port;
            Schema = basePath.Schema;
            Path = path;
        }

        public Uri ToUri()
        {
            return new UriBuilder
            {
                Scheme = Schema,
                Host = Host,
                Path = Path,
                Port = Port
            }.Uri;
        }

        public string ToUriString()
        {
            return new UriBuilder
            {
                Scheme = Schema,
                Host = Host,
                Path = Path,
                Port = Port
            }.ToString();
        }

        public static implicit operator string(UriContainer container)
        {
            return container.ToUriString();
        }

        public static implicit operator Uri(UriContainer container)
        {
            return container.ToUri();
        }
    }
}
