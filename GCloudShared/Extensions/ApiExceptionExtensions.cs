using System;
using System.Collections.Generic;
using System.Text;
using GCloud.Shared.Exceptions;
using Newtonsoft.Json;
using Refit;
using Optional;

namespace GCloudShared.Extensions
{
    public static class ApiExceptionExtensions
    {
        public static Option<ExceptionHandlerResult> GetApiErrorResult(this ApiException apiException)
        {
            var result = Option.None<ExceptionHandlerResult>();

            if (apiException.HasContent)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(apiException.Content).SomeNotNull();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return result;
        }
    }
}
