using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GCloudShared.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static bool TryDeserializeObject<TTarget>(this string json, out TTarget result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<TTarget>(json);
                return true;
            }
            catch(Exception ex)
            {
                result = default(TTarget);
                return false;
            }
        }
    }
}
