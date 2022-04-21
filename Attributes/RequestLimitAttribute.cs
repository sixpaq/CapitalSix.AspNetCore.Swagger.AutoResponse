using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace CapitalSix.AspNetCore.Swagger.AutoResponse
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestLimitAttribute : ActionFilterAttribute
    {
        private readonly string _name;

        public RequestLimitAttribute(string name)
        {
            _name = name;
        }

        public int NumberOfRequest { get; set; } = 1;
        public int Seconds { get; set; } = 1;

        private static MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ipAddress = context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
            var memoryCacheKey = $"{_name}-{ipAddress}";
            Cache.TryGetValue(memoryCacheKey, out int prevReqCount);
            if (prevReqCount >= NumberOfRequest)
            {
                context.Result = new ContentResult
                {
                    Content = $"Request limit is exceeded. Try again in {Seconds} seconds."
                };
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.TooManyRequests;
            }
            else
            {
                var cacheEntryOptions =
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(Seconds));
                Cache.Set(memoryCacheKey, prevReqCount + 1, cacheEntryOptions);
            }
        }
    }
}