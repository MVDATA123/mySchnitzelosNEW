using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using GCloud.Shared.Dto.Domain.CouponUsageAction;
using GCloud.Shared.Dto.Domain.CouponUsageRequirement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;

namespace GCloudShared.Shared
{
    public class HttpClientContainer
    {
        private static readonly HttpClientContainer _instance = new HttpClientContainer();
        private HttpClient _client;
        private MyHttpClientHandler _handler;
        private CookieContainer _cookieContainer;

        public RefitSettings RefitSettings => new RefitSettings()
        {
            JsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new ShouldDeserializeContractResolver()
            }
        };

        private HttpClientContainer()
        {
            //odkomentarisati kada se radi na lokalu
            // IP address of the local PC
            //string uriString = "http://192.168.1.6/GCloud";
            //Uri uri = new Uri(uriString);

            _cookieContainer = new CookieContainer();
            _handler = new MyHttpClientHandler() 
            { 
                CookieContainer = _cookieContainer 
            };
            _client = new HttpClient(_handler) 
            {
                BaseAddress = UriContainer.BasePath,
                //odkomentarisati kada se radi na lokalu
                //BaseAddress = uri,
                DefaultRequestHeaders =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("application/json")
                    }
                }
            };
        }

        public void SetInternetState(IInternetState internetState)
        {
            _handler.InternetState = internetState;
        }
        public static HttpClientContainer Instance => _instance;

        public HttpClient HttpClient => _client;

        public CookieContainer CookieContainer => _cookieContainer;

        private class ShouldDeserializeContractResolver : DefaultContractResolver
        {
            public new static readonly ShouldDeserializeContractResolver Instance = new ShouldDeserializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType == typeof(List<AbstractUsageActionDto>) || property.PropertyType == typeof(List<AbstractUsageRequirementDto>))
                {
                    property.ShouldDeserialize = o => false;
                }

                return property;
            }
        }
    }
}
